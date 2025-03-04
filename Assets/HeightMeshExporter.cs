using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HeightMeshExporter : MonoBehaviour
{
    [SerializeField] bool createDebugMesh = false;
    [SerializeField] string filename;
    [SerializeField] bool invertZ;

    void Start()
    {
        ExtractToFile(filename);
    }

    class NavTri
    {
        public int[] indices = new int[3];
        public int[] neighbours = new int[3];
        public int neighbourCount = 0;

        public NavTri()
        {
            indices[0] = indices[1] = indices[2] = -1;
            neighbours[0] = neighbours[1] = neighbours[2] = -1;
        }

        public bool SharesEdgeWith(NavTri t)
        {
            for (int i = 0; i < 3; ++i)
            {
                int a = indices[i];
                int b = indices[(i + 1) % 3];

                if (t.HasEdge(a, b)) return true;
            }
            return false;
        }

        bool HasEdge(int ea, int eb)
        {
            for (int i = 0; i < 3; ++i)
            {
                int a = indices[i];
                int b = indices[(i + 1) % 3];

                if ((a == ea && b == eb) || (b == ea && a == eb)) return true;
            }
            return false;
        }

        public void AddNeighbour(int newIndex)
        {
            if (neighbourCount < 3) neighbours[neighbourCount++] = newIndex;
        }
    };

    void ExtractToFile(string filename)
    {
        NavMeshTriangulation tris = NavMesh.CalculateTriangulation();
        Vector3[] allVerts = tris.vertices;
        int[] allIndices = tris.indices;

        Dictionary<int, int> vertexRemap = new Dictionary<int, int>();
        List<Vector3> adjustedVerts = new List<Vector3>();
        List<NavTri> allTris = new List<NavTri>();

        // ✅ Adjust vertex heights using Unity's real heightmesh
        for (int i = 0; i < allVerts.Length; i++)
        {
            Vector3 correctedVert = allVerts[i];

            NavMeshHit hit;
            if (NavMesh.SamplePosition(correctedVert, out hit, 1.0f, NavMesh.AllAreas))
            {
                correctedVert.y = hit.position.y;  // Use correct height
            }

            vertexRemap[i] = adjustedVerts.Count; // Track index mapping
            adjustedVerts.Add(correctedVert);
        }

        // ✅ Create triangle list
        for (int i = 0; i < allIndices.Length; i += 3)
        {
            NavTri t = new NavTri();

            if (invertZ)
            {
                t.indices[0] = vertexRemap[allIndices[i + 0]];
                t.indices[1] = vertexRemap[allIndices[i + 2]];
                t.indices[2] = vertexRemap[allIndices[i + 1]];
            }
            else
            {
                t.indices[0] = vertexRemap[allIndices[i + 0]];
                t.indices[1] = vertexRemap[allIndices[i + 1]];
                t.indices[2] = vertexRemap[allIndices[i + 2]];
            }

            allTris.Add(t);
        }

        // ✅ Compute neighbor relationships
        for (int j = 0; j < allTris.Count; ++j)
        {
            if (allTris[j].neighbourCount == 3) continue;

            for (int i = j + 1; i < allTris.Count; ++i)
            {
                if (allTris[i].neighbourCount == 3) continue;

                if (allTris[j].SharesEdgeWith(allTris[i]))
                {
                    allTris[j].AddNeighbour(i);
                    allTris[i].AddNeighbour(j);
                }
            }
        }

        // ✅ Write full NavMesh data to file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
        {
            file.Write(adjustedVerts.Count + "\n");
            file.Write(allTris.Count * 3 + "\n");

            // ✅ Write vertices with real height values
            foreach (Vector3 v in adjustedVerts)
            {
                file.Write(v.x + " " + v.y + " " + (invertZ ? -v.z : v.z) + "\n");
            }

            // ✅ Write triangles
            foreach (NavTri t in allTris)
            {
                file.Write(t.indices[0] + " " + t.indices[1] + " " + t.indices[2] + "\n");
            }

            // ✅ Write neighbors
            foreach (NavTri t in allTris)
            {
                file.Write(t.neighbours[0] + " " + t.neighbours[1] + " " + t.neighbours[2] + "\n");
            }
        }

        // ✅ Create Debug Mesh if needed
        if (createDebugMesh)
        {
            Mesh m = new Mesh();
            m.SetVertices(adjustedVerts);
            m.SetIndices(allIndices, MeshTopology.Triangles, 0);

            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            mf.mesh = m;
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        }
    }
}
