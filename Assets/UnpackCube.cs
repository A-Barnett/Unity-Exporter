using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class UnpackCube : MonoBehaviour
{
    [SerializeReference] public List<GameObject> cubeSectionsObj;
    [SerializeReference] public List<Transform> cubeSectionsTransform;
    [SerializeReference] public List<Transform> cubeSectionsUnpackedTransform;
    [SerializeReference] public float lerpTime;

    private bool isUnpacking = false;
    private bool isPacking = false;
    private float elapsedTime = 0f;
    private bool packed;
    private bool exported = false;
    public void quickPack()
    {
        int finalCount = 0;
        foreach (GameObject obj in cubeSectionsObj)
        {
            obj.transform.position = cubeSectionsTransform[finalCount].position;
            obj.transform.rotation = cubeSectionsTransform[finalCount].rotation;
            finalCount++;
        }
    }
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    public void unpack()
    {
        isUnpacking = true;
        isPacking = false;
        elapsedTime = 0f;
    }

    public void pack()
    {
        isPacking = true;
        isUnpacking = false;
        elapsedTime = 0f;
    }

    private void EditorUpdate()
    {
        if (isUnpacking)
        {
         

            int count = 0;
            foreach (GameObject obj in cubeSectionsObj)
            {
                obj.transform.position = Vector3.Lerp(cubeSectionsTransform[count].position, cubeSectionsUnpackedTransform[count].position, elapsedTime / lerpTime);
                if (cubeSectionsUnpackedTransform[count].position.Equals(new Vector3(0, 0, 240)))
                {
                    obj.transform.rotation = Quaternion.Slerp(Quaternion.Inverse(cubeSectionsTransform[count].rotation), Quaternion.Inverse(cubeSectionsUnpackedTransform[count].rotation), elapsedTime / lerpTime);
                }
                else
                {
                    obj.transform.rotation = Quaternion.Lerp(cubeSectionsTransform[count].rotation, cubeSectionsUnpackedTransform[count].rotation, elapsedTime / lerpTime);
                }
                count++;
            }

            if (elapsedTime >= lerpTime)
            {
                isUnpacking = false;
                int finalCount = 0;
                foreach (GameObject obj in cubeSectionsObj)
                {
                    obj.transform.position = cubeSectionsUnpackedTransform[finalCount].position;
                    obj.transform.rotation = cubeSectionsUnpackedTransform[finalCount].rotation;
                    finalCount++;
                }

                elapsedTime = 0;
            }
        }

        if (isPacking)
        {

            int count = 0;
            foreach (GameObject obj in cubeSectionsObj)
            {
                obj.transform.position = Vector3.Lerp(cubeSectionsUnpackedTransform[count].position, cubeSectionsTransform[count].position, elapsedTime / lerpTime);
                if (cubeSectionsUnpackedTransform[count].position.Equals(new Vector3(0, 0, 240)))
                {
                    obj.transform.rotation = Quaternion.Slerp(Quaternion.Inverse(cubeSectionsUnpackedTransform[count].rotation), Quaternion.Inverse(cubeSectionsTransform[count].rotation), elapsedTime / lerpTime);
                }
                else
                {
                    obj.transform.rotation = Quaternion.Lerp(cubeSectionsUnpackedTransform[count].rotation, cubeSectionsTransform[count].rotation, elapsedTime / lerpTime);
                }

                count++;
            }

            if (elapsedTime >= lerpTime)
            {
                isPacking = false;
                int finalCount = 0;
                foreach (GameObject obj in cubeSectionsObj)
                {
                    obj.transform.position = cubeSectionsTransform[finalCount].position;
                    obj.transform.rotation = cubeSectionsTransform[finalCount].rotation;
                    finalCount++;
                }

                elapsedTime = 0;
            }
        }
        elapsedTime += Time.deltaTime;
    }
    
}
