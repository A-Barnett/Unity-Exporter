using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class UnpackCube : MonoBehaviour
{
    
    [SerializeReference] public List<GameObject> cubeSectionsObj;
    [SerializeReference] public List<Transform> cubeSectionsTransform;
    [SerializeReference] public List<Transform> cubeSectionsUnpackedTransform;
    
    
    public void unpack()
    {
        int count = 0;
        foreach (GameObject obj in cubeSectionsObj)
        {
            obj.transform.position = cubeSectionsUnpackedTransform[count].position;
            obj.transform.rotation = cubeSectionsUnpackedTransform[count].rotation;
            count++;
        }
    }

    public void pack()
    {
        int count = 0;
        foreach (GameObject obj in cubeSectionsObj)
        {
            obj.transform.position = cubeSectionsTransform[count].position;
            obj.transform.rotation = cubeSectionsTransform[count].rotation;
            count++;
        }
    }
}
