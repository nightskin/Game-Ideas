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
            level.seed = System.DateTime.Now.ToString();
            if(level.type == LevelType.DUNGEON)
            {
                Random.InitState(level.seed.GetHashCode());
                level.GenerateDungeonData(level.useBoxShapedRooms);
            }
            
            if(level.transform.childCount > 0)
            {
                for(int i = 0; i < level.transform.childCount; i++)
                {
                    Chunk chunk = level.transform.GetChild(i).GetComponent<Chunk>();
                    chunk.Generate();
                }
            }
            else
            {
                level.Init(true);
            }
        }
        if(GUILayout.Button("Clear"))
        {
            for(int i = 0; i < level.transform.childCount; i++)
            {
                DestroyImmediate(level.transform.GetChild(i).gameObject);
            }
        }
    }
}
