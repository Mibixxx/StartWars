using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform rallyPoint;
    public Vector3 rallyFacingDirection = Vector3.forward; // puoi impostarla via Inspector
    public float spawnInterval = 5f;
    public int enemiesPerRow = 3;
    public float spacing = 4f;

    private float timer = 0f;
    private readonly System.Collections.Generic.List<TestEnemyUnit> activeEnemies = new();
    private bool waitingToRespawn = false;

    void Update()
    {
        if (waitingToRespawn)
        {
            // Aspetta che tutti i nemici siano morti
            if (activeEnemies.Count == 0)
            {
                timer += Time.deltaTime;
                if (timer >= spawnInterval)
                {
                    SpawnEnemyGrid(4);
                    timer = 0f;
                }
            }
        }
        else
        {
            // Primo spawn (iniziale)
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnEnemyGrid(4);
                timer = 0f;
            }
        }
    }

    private System.Collections.IEnumerator DelayedStart(TestEnemyUnit enemy, Vector3 rallyPos)
    {
        yield return null; // aspetta un frame per assicurarsi che l'agente sia pronto
        enemy.MoveToRallyPoint(rallyPos);
        enemy.SetFacingDirection(rallyFacingDirection);
    }

    void SpawnEnemyGrid(int enemyCount)
    {
        Vector3 baseSpawnPos = spawnPoint.position;
        Vector3 baseRallyPos = rallyPoint.position;

        for (int i = 0; i < enemyCount; i++)
        {
            int col = i % enemiesPerRow;
            int row = i / enemiesPerRow;

            float xOffset = (col - (enemiesPerRow - 1) / 2f) * spacing;
            float zOffset = row * spacing;

            Vector3 spawnPos = baseSpawnPos + new Vector3(xOffset, 0, zOffset);
            Vector3 rallyPos = RallyPointManager.GetFreePosition(baseRallyPos, spacing, enemiesPerRow);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.Euler(0, 180, 0));
            TestEnemyUnit enemyUnit = enemy.GetComponent<TestEnemyUnit>();

            if (enemyUnit != null)
            {
                activeEnemies.Add(enemyUnit);
                enemyUnit.OnDeath += HandleEnemyDeath;
                StartCoroutine(DelayedStart(enemyUnit, rallyPos));
            }
        }

        waitingToRespawn = true;
    }

    private void HandleEnemyDeath(TestEnemyUnit enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}
