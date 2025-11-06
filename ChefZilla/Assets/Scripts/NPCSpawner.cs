using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform doorOutside;  // << NOVO
    public Transform exitPoint;
    public float interval = 4f;
    public int maxAlive = 6;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval && CountAlive() < maxAlive)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    int CountAlive()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<CustomerAI>(FindObjectsSortMode.None).Length;
#else
        return Object.FindObjectsOfType<CustomerAI>().Length;
#endif
    }

    void SpawnOne()
    {
        if (!customerPrefab || !spawnPoint) return;

        var go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);

        var ai = go.GetComponent<CustomerAI>();
        if (ai)
        {
            ai.exitPoint = exitPoint;
            ai.doorOutside = doorOutside;
            ai.mustEnterThroughDoor = true;
        }

        var ag = go.GetComponent<NavMeshAgent>();
        if (ag && NavMesh.SamplePosition(spawnPoint.position, out var hit, 2f, NavMesh.AllAreas))
            ag.Warp(hit.position);
    }
}
