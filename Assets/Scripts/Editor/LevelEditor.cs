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
            if(level.transform.childCount > 0) level.InvokeNextFrame(() => level.DestroyKids());
            level.Generate(true);
        }
    }
}
