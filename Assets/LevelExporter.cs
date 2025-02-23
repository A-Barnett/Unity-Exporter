using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LevelExporter : MonoBehaviour
{
    // NOTE: Null strings will be serialised as empty strings
    class ObjectData
    {
        // Object transform
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        // Collider transform
        public Vector3 colliderPosition;
        public Vector3 colliderScale;

        // Other
        public string meshName;
        public string mainTextureName;
        public string normalTextureName;

        public char type;
        public override string ToString()
        {
            return $"Mesh: {meshName}, Texture: {mainTextureName}, Normal Texture: {normalTextureName}\n" +
                   $"Position: {position}, Rotation: {rotation}, Scale: {scale}" +
                   $" Collider Position: {colliderPosition}, Collider Scale: {colliderScale}";
        }
    }
    public UnpackCube packer;

    public string OutputFileName = "level_1.json";

    public bool customPath;
    public string SavePath = "Path/to/TeamProject/Assets/Levels";

    public void Start()
    {
        packer.quickPack();
        List<ObjectData> objects = new List<ObjectData>();
        FindLeafNodes(transform, objects);

        Debug.Log("Total leaf nodes found: " + objects.Count);

        SaveToJson(objects);
    }

    public void SaveLevel()
    {
        packer.quickPack();
        List<ObjectData> objects = new List<ObjectData>();
        FindLeafNodes(transform, objects);

        Debug.Log("Total leaf nodes found: " + objects.Count);

        SaveToJson(objects);
    }

    void FindLeafNodes(Transform node, List<ObjectData> objects)
    {
        if (node.childCount > 0)
        {
            foreach (Transform child in node)
            {
                FindLeafNodes(child, objects);
            }
        }
        else
        {
            MeshFilter meshFilter = node.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                ObjectData objectData = new ObjectData
                {
                    position = node.position,
                    rotation = node.rotation,
                    scale = node.lossyScale,
                    meshName = meshFilter.sharedMesh?.name,
                };
                // Ensure the node has a GameObject component
                GameObject nodeGameObject = node.gameObject;
                ObjectAttributes objectAttributes = nodeGameObject.GetComponent<ObjectAttributes>();
                objectData.type = objectAttributes.getType();
                // Get collider info if available
                BoxCollider box = node.GetComponent<BoxCollider>();
                if (box != null)
                {
                    objectData.colliderPosition = box.center;
                    objectData.colliderScale = box.size;
                }

                // Get texture info if available
                MeshRenderer meshRenderer = node.GetComponent<MeshRenderer>();

                objectData.mainTextureName = meshRenderer?.material?.mainTexture?.name;
                objectData.normalTextureName = meshRenderer?.material?.GetTexture("_BumpMap")?.name;

                objects.Add(objectData);
            }
        }
    }

    void SaveToJson(List<ObjectData> objects)
    {
        string folderPath = "";
        if (customPath)
        {
            folderPath = SavePath;
        }
        else
        {
            folderPath = Path.Combine(Application.dataPath, "LevelData");
        }
    
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, OutputFileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var obj in objects)
            {
                string json = JsonUtility.ToJson(obj);
                writer.WriteLine(json);
            }
        }

        Debug.Log("Data saved to " + filePath);
    }
    
}
