using UnityEngine;
using System.Collections.Generic;

public class HouseSpawner : MonoBehaviour
{
    [Header("Terrain")]
    public Terrain terrain;
    public TerrainData terrainData;

    [Header("Prefabs")]
    public GameObject[] housePrefabs;
    public GameObject roadPrefab;

    [Header("Spawn Area")]
    public int jumlahRumah = 20;
    public float radius = 60f;
    public float gridSize = 12f;

    [Header("Validation")]
    public float maxSlope = 25f;
    public float obstacleCheckRadius = 6f;
    public LayerMask obstacleLayer;

    [Header("Texture Filter")]
    public int allowedTextureIndex = 0;
    public float minTextureStrength = 0.5f;

    private List<GameObject> spawned = new List<GameObject>();
    private List<Vector3> housePositions = new List<Vector3>();

    void Reset()
    {
        terrain = Terrain.activeTerrain;
        if (terrain != null)
            terrainData = terrain.terrainData;
    }

    // =========================
    // EDITOR ENTRY POINT
    // =========================
    public void SpawnInEditor()
    {
        ClearSpawned();
        housePositions.Clear();

        int spawnedCount = 0;
        int attempts = 0;

        while (spawnedCount < jumlahRumah && attempts < jumlahRumah * 30)
        {
            attempts++;

            Vector3 pos;
            if (!TryGetValidPosition(out pos))
                continue;

            GameObject prefab =
                housePrefabs[Random.Range(0, housePrefabs.Length)];

            GameObject house =
                UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            house.transform.position = pos;
            house.transform.rotation =
                Quaternion.Euler(0, Random.Range(0, 360), 0);

            spawned.Add(house);
            housePositions.Add(pos);
            spawnedCount++;
        }

        GenerateRoads();
    }

    public void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
                DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }

    // =========================
    // POSITION CORE (FIX)
    // =========================
    bool TryGetValidPosition(out Vector3 finalPos)
    {
        finalPos = Vector3.zero;

        Vector3 rnd = Random.insideUnitSphere * radius;
        rnd.y = 0;

        // GRID SNAP
        rnd.x = Mathf.Round(rnd.x / gridSize) * gridSize;
        rnd.z = Mathf.Round(rnd.z / gridSize) * gridSize;

        // FINAL WORLD POS
        Vector3 pos = transform.position + rnd;

        // 🔴 FIX 1: RADIUS CHECK (FINAL)
        if (Vector3.Distance(transform.position, pos) > radius)
            return false;

        // 🔴 FIX 2: TERRAIN BOUNDS CHECK
        Vector3 tp = pos - terrain.transform.position;
        if (tp.x < 0 || tp.z < 0 ||
            tp.x > terrainData.size.x ||
            tp.z > terrainData.size.z)
            return false;

        // HEIGHT
        float h = terrain.SampleHeight(pos);
        pos.y = h + terrain.transform.position.y;

        // VALIDATION
        if (!IsSlopeValid(pos)) return false;
        if (!IsTextureValid(pos)) return false;
        if (IsBlockedByObstacle(pos)) return false;

        finalPos = pos;
        return true;
    }

    // =========================
    // VALIDATION
    // =========================
    bool IsSlopeValid(Vector3 worldPos)
    {
        Vector3 tp = worldPos - terrain.transform.position;
        float nx = tp.x / terrainData.size.x;
        float nz = tp.z / terrainData.size.z;

        float slope = terrainData.GetSteepness(nx, nz);
        return slope <= maxSlope;
    }

    bool IsTextureValid(Vector3 worldPos)
    {
        Vector3 tp = worldPos - terrain.transform.position;

        int mapX = Mathf.Clamp(
            Mathf.FloorToInt(tp.x / terrainData.size.x * terrainData.alphamapWidth),
            0, terrainData.alphamapWidth - 1
        );

        int mapZ = Mathf.Clamp(
            Mathf.FloorToInt(tp.z / terrainData.size.z * terrainData.alphamapHeight),
            0, terrainData.alphamapHeight - 1
        );

        float[,,] splat = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
        return splat[0, 0, allowedTextureIndex] >= minTextureStrength;
    }

    bool IsBlockedByObstacle(Vector3 pos)
    {
        return Physics.OverlapSphere(
            pos,
            obstacleCheckRadius,
            obstacleLayer
        ).Length > 0;
    }

    // =========================
    // ROADS
    // =========================
    void GenerateRoads()
    {
        if (roadPrefab == null || housePositions.Count < 2)
            return;

        for (int i = 0; i < housePositions.Count - 1; i++)
        {
            Vector3 a = housePositions[i];
            Vector3 b = housePositions[i + 1];

            Vector3 mid = (a + b) * 0.5f;
            float dist = Vector3.Distance(a, b);

            GameObject road =
                UnityEditor.PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject;

            road.transform.position = mid;
            road.transform.LookAt(b);
            road.transform.localScale =
                new Vector3(4f, 1f, dist);

            spawned.Add(road);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
