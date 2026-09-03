using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelMeshGenerator))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LevelMeshGenerator level = (LevelMeshGenerator)target;
        if(GUILayout.Button("Create Random World"))
        {
            level.Generate();
        }
        if(GUILayout.Button("Clear Child GameObjects"))
        {
            for(int i = 0; i < level.transform.childCount; i++)
            {
                GameObject child = level.transform.GetChild(i).gameObject;
                while(child)
                {
                    DestroyImmediate(child);
                }
            }
        }
    }
}
