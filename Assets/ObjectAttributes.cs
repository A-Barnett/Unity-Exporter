using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectAttributes : MonoBehaviour
{

    [SerializeField] private ObjectType objType;
    [SerializeField] public float jumpPadStrength = 0.0f;
    //jumpPadStrength
    public enum ObjectType
    {
        Default,
        Floor,
        JumpPad,
        Slime,
        Ice,
        PointLight,
        RespawnPoint,
        Player,
        Centre,
        SlimeCastle,
        Courtyard
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
