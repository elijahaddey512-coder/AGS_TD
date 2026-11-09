using UnityEngine;

public class enemySpawn : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] float spawnTimer = 2f;

    void Start()
    {
        StartSpawing();
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, enemies.Length); // spawns randoms
        Instantiate(enemies[randomIndex], transform.position, Quaternion.identity);//spawns objects
    }
    void StopSpawning()
    {
        CancelInvoke();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void StartSpawing()
    {
        InvokeRepeating("SpawnEnemy", spawnTimer, spawnTimer);// starts and continue spawns
    }

    // Update is called once per frame
    void Update()
    {

    }
}
