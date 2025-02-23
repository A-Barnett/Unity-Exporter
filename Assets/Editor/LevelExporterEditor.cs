
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelExporter))]
public class LevelExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw default script properties

        LevelExporter script = (LevelExporter)target;
        if (GUILayout.Button("\n\nSave Level\n\n"))
        {
            script.SaveLevel();
        }
        
    }
}