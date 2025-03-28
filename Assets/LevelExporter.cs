﻿using System.Collections.Generic;
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
        public float jumpPadStrength;

        public ObjectAttributes.ObjectType type;
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
    
    public Material defaultMaterial;
    public Material floorMaterial;
    public Material jumpPadMaterial;
    public Material slimeMaterial;
    public Material iceMaterial;
    public Material centreMaterial;
    public Material slimeCastleMaterial;
    public Material courtyardMaterial;
    public Material jumpRoomMaterial;
    public Material jumpFloorMaterial;
    public Material zigZagMaterial;
    private Dictionary<ObjectAttributes.ObjectType, Material> materialMap;
    public void Start()
    {
        SaveLevel();
    }

    public void SaveLevel()
    {
        packer.quickPack();
        List<ObjectData> objects = new List<ObjectData>();
        FindLeafNodes(transform, objects);

        Debug.Log("Total leaf nodes found: " + objects.Count);

        SaveToJson(objects);
    }

    public void SetMaterials()
    {
        materialMap = new Dictionary<ObjectAttributes.ObjectType, Material>
        {
            { ObjectAttributes.ObjectType.Default, defaultMaterial },
            { ObjectAttributes.ObjectType.Floor, floorMaterial },
            { ObjectAttributes.ObjectType.JumpPad, jumpPadMaterial },
            { ObjectAttributes.ObjectType.Slime, slimeMaterial },
            { ObjectAttributes.ObjectType.Ice, iceMaterial },
            { ObjectAttributes.ObjectType.PointLight, defaultMaterial },
            { ObjectAttributes.ObjectType.RespawnPoint, defaultMaterial },
            { ObjectAttributes.ObjectType.Centre, centreMaterial },
            { ObjectAttributes.ObjectType.SlimeCastle, slimeCastleMaterial },
            { ObjectAttributes.ObjectType.Courtyard, courtyardMaterial }, 
            { ObjectAttributes.ObjectType.JumpRoom, jumpRoomMaterial },
            { ObjectAttributes.ObjectType.JumpRoomFloor, jumpFloorMaterial },
            { ObjectAttributes.ObjectType.ZigZag, zigZagMaterial }
        };
        List<ObjectData> objects = new List<ObjectData>();
        UpdateMaterials(transform, objects);
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
                objectData.type = objectAttributes.GetTypeEnum();
                objectData.jumpPadStrength = objectAttributes.jumpPadStrength;
      
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
    
    void UpdateMaterials(Transform node, List<ObjectData> objects)
    {
        if (node.childCount > 0)
        {
            foreach (Transform child in node)
            {
                UpdateMaterials(child, objects);
            }
        }
        else
        {
            MeshFilter meshFilter = node.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                GameObject nodeGameObject = node.gameObject;
                MeshRenderer meshRenderer = node.GetComponent<MeshRenderer>();
                meshRenderer.material = materialMap[nodeGameObject.GetComponent<ObjectAttributes>().GetTypeEnum()];
                if (nodeGameObject.GetComponent<ObjectAttributes>().GetTypeEnum() == ObjectAttributes.ObjectType.Slime)
                {
                    meshRenderer.material.color = Color.green;;
                }
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
