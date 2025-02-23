using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAttributes : MonoBehaviour
{
    [SerializeField] private char type;

    public char getType()
    {
        return type;
    }

    public void setType(char typeIn)
    {
        type = typeIn;
    }
}
