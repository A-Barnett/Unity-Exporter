using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectAttributes : MonoBehaviour
{

    [SerializeField] private ObjectType objType;
    
    //
    public enum ObjectType
    {
        Default,
        Floor,
        JumpPad,
        Slime, 
        Ice,
        PointLight,
        RespawnPoint
    }
    
    public ObjectType GetTypeEnum()
    {
        return objType;
    }
    public void SetTypeEnum(ObjectType objTypeIn)
    {
         objType = objTypeIn;
    }

    
}
