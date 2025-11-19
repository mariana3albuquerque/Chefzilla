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
    public Transform exitPivotTop;    // corredor superior
    public Transform exitPivotBottom; // corredor inferior

    [Tooltip("Intervalo base de spawn (usado se a progressão estiver desligada).")]
    public float interval = 4f;
    [Tooltip("Máximo base de clientes vivos (usado se a progressão estiver desligada).")]
    public int maxAlive = 6;

    [Header("Progressão de dificuldade por tempo")]
    [Tooltip("Se true, usa o tempo do GameTimer para ir deixando o spawn mais difícil ao longo da partida.")]
    public bool usarDificuldadePorTempo = true;

    [Tooltip("Intervalo de spawn no início da partida (fácil).")]
    public float easyInterval = 5f;

    [Tooltip("Intervalo mínimo de spawn no fim da partida (difícil).")]
    public float hardInterval = 2f;

    [Tooltip("Máximo de clientes simultâneos no início da partida.")]
    public int easyMaxAlive = 3;

    [Tooltip("Máximo de clientes simultâneos no fim da partida.")]
    public int hardMaxAlive = 7;

    [Header("Tutorial / Fluxo de jogo")]
    [Tooltip("Se marcado, já começa spawnando assim que a cena carregar.")]
    public bool spawnOnStart = true; // se false, fica travado até EnableSpawning()

    bool canSpawn;
    float timer;

    void Awake()
    {
        canSpawn = spawnOnStart;
    }

    void Update()
    {
        if (!canSpawn) return;

        // valores padrão, caso a dificuldade dinâmica esteja desligada
        float intervaloAtual = interval;
        int maxAliveAtual = maxAlive;
        float dificuldade01 = 0f;

        if (usarDificuldadePorTempo)
        {
            var gt = GameTimer.I;
            if (gt != null && gt.initialSeconds > 0)
            {
                float total = gt.initialSeconds;
                float elapsed = total - gt.Remaining;
                dificuldade01 = Mathf.Clamp01(elapsed / total);

                // 0 → easyInterval / easyMaxAlive
                // 1 → hardInterval / hardMaxAlive
                intervaloAtual = Mathf.Lerp(easyInterval, hardInterval, dificuldade01);
                maxAliveAtual = Mathf.RoundToInt(Mathf.Lerp(easyMaxAlive, hardMaxAlive, dificuldade01));
            }
        }

        timer += Time.deltaTime;
        if (timer >= intervaloAtual && CountAlive() < maxAliveAtual)
        {
            timer = 0f;
            SpawnOne(dificuldade01);
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

    void SpawnOne(float dificuldade01)
    {
        if (!spawnPoint) return;

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogWarning("NPCSpawner: nenhum prefab configurado em 'customerPrefabs'.");
            return;
        }

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

            // aplica dificuldade nesse cliente
            ai.ApplyDifficulty(dificuldade01);
        }

        var ag = go.GetComponent<NavMeshAgent>();
        if (ag && NavMesh.SamplePosition(spawnPoint.position, out var hit, 2f, NavMesh.AllAreas))
            ag.Warp(hit.position);
    }

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
