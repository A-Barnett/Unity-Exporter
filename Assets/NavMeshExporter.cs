using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;

public class NavMeshExporter : MonoBehaviour
{
    [SerializeField] private bool createDebugMesh = false;
    [SerializeField] private string filename;
    [SerializeField] private bool invertZ;
    [SerializeField] private NavMeshSurface navMeshSurface;  // ✅ Reference to NavMeshSurface

    void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface is not assigned!");
            return;
        }

        navMeshSurface.BuildNavMesh(); // ✅ Ensure NavMesh is up-to-date
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

        public bool HasIndex(int index)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (indices[i] == index) return true;
            }
            return false;
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

                if ((a == ea && b == eb) || (b == ea && a == eb))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddNeighbour(int newIndex)
        {
            if (neighbourCount == 3)
            {
                Debug.LogWarning("Tri has too many neighbours?");
                return;
            }
            neighbours[neighbourCount++] = newIndex;
        }
    };

    void ExtractToFile(string filename)
    {
        NavMeshTriangulation tris = NavMesh.CalculateTriangulation();
        Vector3[] allVerts = tris.vertices;
        int[] allIndices = tris.indices;

        Weld(allVerts, allIndices);  // ✅ Ensures shared vertices are merged correctly

        List<Vector3> adjustedVerts = new List<Vector3>();

        // ✅ Fix: Adjust vertices by sampling the correct height from the Unity NavMeshSurface
        for (int i = 0; i < allVerts.Length; i++)
        {
            Vector3 correctedVert = allVerts[i];

            // Sample the correct height from the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(correctedVert, out hit, 1.0f, NavMesh.AllAreas))
            {
                correctedVert.y = hit.position.y;  // ✅ Use correct height
            }

            adjustedVerts.Add(correctedVert);
        }

        List<NavTri> allTris = new List<NavTri>();

        for (int i = 0; i < allIndices.Length; i += 3)
        {
            NavTri t = new NavTri();
            if (invertZ)
            {
                t.indices[0] = allIndices[i + 0];
                t.indices[1] = allIndices[i + 2];
                t.indices[2] = allIndices[i + 1];
            }
            else
            {
                t.indices[0] = allIndices[i + 0];
                t.indices[1] = allIndices[i + 1];
                t.indices[2] = allIndices[i + 2];
            }
            allTris.Add(t);
        }

        // ✅ Neighbors remain unchanged
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

        using (StreamWriter file = new StreamWriter(filename))
        {
            file.WriteLine(adjustedVerts.Count);
            file.WriteLine(allIndices.Length);

            // ✅ Write vertices with corrected heights
            foreach (Vector3 v in adjustedVerts)
            {
                string s = $"{v.x} {v.y} {(invertZ ? -v.z : v.z)}";
                file.WriteLine(s);
            }

            // ✅ Write triangle indices
            foreach (NavTri t in allTris)
            {
                string s = $"{t.indices[0]} {t.indices[1]} {t.indices[2]}";
                file.WriteLine(s);
            }

            // ✅ Write neighbor data
            foreach (NavTri t in allTris)
            {
                string s = $"{t.neighbours[0]} {t.neighbours[1]} {t.neighbours[2]}";
                file.WriteLine(s);
            }
        }

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

    void Weld(Vector3[] verts, int[] indices, float threshold = 0.0001f)
    {
        int weldCount = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            Vector3 iv = verts[indices[i]];
            for (int j = i + 1; j < indices.Length; j++)
            {
                if (indices[j] == indices[i]) continue;

                Vector3 jv = verts[indices[j]];
                if ((iv - jv).magnitude < threshold)
                {
                    indices[j] = indices[i];
                    weldCount++;
                    break;
                }
            }
        }
        Debug.Log($"Welded {weldCount} vertices");
    }
}