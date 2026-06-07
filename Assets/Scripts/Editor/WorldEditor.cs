using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        World world = (World)target;
        if(GUILayout.Button("Create Random World"))
        {
            world.CreateRandom();
        }
    }
}
