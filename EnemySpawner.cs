using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Drag your Enemy Prefab here
    public float spawnRate = 1.5f; // How often an enemy spawns (in seconds)
    public float spawnY = 6f;      // The height they spawn at (top of screen)
    public float minX = -8f;       // Furthest left they can spawn
    public float maxX = 8f;        // Furthest right they can spawn

    private float nextSpawnTime = 0f;

    void Update()
    {
        // Check if it is time to spawn a new enemy
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // Pick a random X position between the left and right edges
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        // Spawn the enemy!
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}