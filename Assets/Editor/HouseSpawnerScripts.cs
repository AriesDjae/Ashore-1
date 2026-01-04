using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HouseSpawner))]
public class HouseSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HouseSpawner spawner = (HouseSpawner)target;

        if (GUILayout.Button("SPAWN HOUSES (EDITOR)"))
            spawner.SpawnInEditor();

        if (GUILayout.Button("CLEAR SPAWNED"))
            spawner.ClearSpawned();
    }
}
