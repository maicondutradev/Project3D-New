using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform[] allPatrolPoints;
    public int pointsPerEnemy = 3;
    public float spawnInterval = 5f;
    public int maxEnemies = 10;
    public float navMeshSnapDistance = 2f;

    private float spawnTimer = 0f;
    private Camera mainCamera;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (activeEnemies.Count >= maxEnemies) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            TrySpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void TrySpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform sp in spawnPoints)
        {
            if (!IsPointVisible(sp.position))
            {
                validSpawnPoints.Add(sp);
            }
        }

        if (validSpawnPoints.Count == 0) return;

        Transform chosenSpawn = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

        NavMeshHit hit;
        if (NavMesh.SamplePosition(chosenSpawn.position, out hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            GameObject newEnemy = Instantiate(enemyPrefab, hit.position, chosenSpawn.rotation);
            activeEnemies.Add(newEnemy);

            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null && allPatrolPoints.Length > 0)
            {
                int pointsToAssign = Mathf.Min(pointsPerEnemy, allPatrolPoints.Length);
                Transform[] randomPoints = new Transform[pointsToAssign];

                List<Transform> tempPoints = new List<Transform>(allPatrolPoints);
                for (int i = 0; i < pointsToAssign; i++)
                {
                    int randIndex = Random.Range(0, tempPoints.Count);
                    randomPoints[i] = tempPoints[randIndex];
                    tempPoints.RemoveAt(randIndex);
                }

                enemyScript.patrolPoints = randomPoints;
            }
        }
        else
        {
            Debug.LogWarning("O ponto " + chosenSpawn.name + " esta muito longe do chao navegavel (NavMesh) e foi ignorado.");
        }
    }

    private bool IsPointVisible(Vector3 point)
    {
        if (mainCamera == null) return false;

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(point);

        return viewportPoint.z > 0 && viewportPoint.x > -0.1f && viewportPoint.x < 1.1f && viewportPoint.y > -0.1f && viewportPoint.y < 1.1f;
    }
}