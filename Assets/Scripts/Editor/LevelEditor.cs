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
            if(level.transform.childCount > 0)
            {
                for(int i = 0; i < level.transform.childCount; i++)
                {
                    level.seed = System.DateTime.Now.ToString();
                    Random.InitState(level.seed.GetHashCode());
                    if(level.type == LevelType.DUNGEON) level.GenerateDungeonData(level.useBoxShapedRooms);
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
