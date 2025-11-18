using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    [Header("Configuração de spawn")]
    [Tooltip("Lista de prefabs de clientes (Customer01, Customer02, ...). Será sorteado um a cada spawn.")]
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public Transform doorOutside;
    public Transform exitPoint;
    public Transform exitPivotTop;      // corredor superior
    public Transform exitPivotBottom;   // corredor inferior
    public float interval = 4f;
    public int maxAlive = 6;

    [Header("Tutorial / Fluxo de jogo")]
    [Tooltip("Se marcado, já começa spawnando assim que a cena carregar.")]
    public bool spawnOnStart = true;   // se false, fica travado até EnableSpawning()
    bool canSpawn;

    float timer;

    void Awake()
    {
        // define se começa spawnando ou não
        canSpawn = spawnOnStart;
    }

    void Update()
    {
        // durante o tutorial, isso fica falso
        if (!canSpawn) return;

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
        if (!spawnPoint) return;
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogWarning("NPCSpawner: nenhum prefab configurado em 'customerPrefabs'.");
            return;
        }

        // 🔹 Sorteia um prefab da lista
        int idx = Random.Range(0, customerPrefabs.Length);
        GameObject prefab = customerPrefabs[idx];
        if (!prefab) return;

        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        var ai = go.GetComponent<CustomerAI>();
        if (ai)
        {
            ai.exitPoint = exitPoint;
            ai.doorOutside = doorOutside;
            ai.mustEnterThroughDoor = true;
            ai.exitPivotTop = exitPivotTop;
            ai.exitPivotBottom = exitPivotBottom;
        }

        var ag = go.GetComponent<NavMeshAgent>();
        if (ag && NavMesh.SamplePosition(spawnPoint.position, out var hit, 2f, NavMesh.AllAreas))
            ag.Warp(hit.position);
    }

    // --- controle externo (tutorial / jogo valendo) ---

    public void EnableSpawning()
    {
        canSpawn = true;
        timer = 0f;
    }

    public void DisableSpawning()
    {
        canSpawn = false;
    }
}
