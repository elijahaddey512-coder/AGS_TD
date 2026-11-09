
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RandomMonsterSpawner : MonoBehaviour
{
    [Header("Monster Prefabs")]
    [SerializeField] private GameObject[] monsterPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Core References")]
    [SerializeField] private GameObject[] cores;

    private bool spawningStopped = false;

    private void Start()
    {
        StartCoroutine(SpawnMonsters());
    }

    private IEnumerator SpawnMonsters()
    {
        // Wait before spawning the first monster.
        yield return new WaitForSeconds(initialDelay);

        while (!spawningStopped)
        {
            // Check whether all 3 cores are destroyed.
            if (AllCoresDestroyed())
            {
                StopSpawning();
                yield break;
            }

            SpawnRandomMonster();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool AllCoresDestroyed()
    {
        if (cores == null || cores.Length == 0)
            return false;

        foreach (GameObject core in cores)
        {
            // Check whether a core still exists.
            if (core != null && core.activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnRandomMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
            return;

        if (spawnPoint == null)
            return;

        // Choose a random monster.
        int randomIndex = Random.Range(0, monsterPrefabs.Length);

        GameObject selectedMonster = monsterPrefabs[randomIndex];

        if (selectedMonster == null)
            return;

        // Find a valid NavMesh position.
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            spawnPoint.position,
            out hit,
            2f,
            NavMesh.AllAreas))
        {
            Instantiate(
                selectedMonster,
                hit.position,
                spawnPoint.rotation
            );

            Debug.Log("Spawned monster: " + selectedMonster.name);
        }
        else
        {
            Debug.LogWarning("No NavMesh found near the spawn point!");
        }
    }

    private void StopSpawning()
    {
        spawningStopped = true;

        Debug.Log("All 3 Cores destroyed! Monster spawning stopped.");

        // Make the SpawnPoint disappear.
        if (spawnPoint != null)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }
}


