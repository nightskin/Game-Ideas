using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelMeshGenerator))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LevelMeshGenerator level = (LevelMeshGenerator)target;
        if(GUILayout.Button("Create Random"))
        {
            level.DestroyKids();
            level.Generate(true);
        }
        if(GUILayout.Button("Clear"))
        {
            level.DestroyKids();
        }
    }
}
