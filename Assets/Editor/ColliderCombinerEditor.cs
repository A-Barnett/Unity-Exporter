using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColliderCombiner))]
public class ColliderCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw default script properties

        ColliderCombiner script = (ColliderCombiner)target;
        if (GUILayout.Button("Combine Colliders"))
        {
            script.RunCombiner();
            //
        }
    }
}