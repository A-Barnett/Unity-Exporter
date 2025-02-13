using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnpackCube))]
public class UnpackCubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw default script properties

        UnpackCube script = (UnpackCube)target;
        if (GUILayout.Button("\n\nPack/Unpack Cube Map\n\n"))
        {
            script.togglePack();
        }
        
    }
}