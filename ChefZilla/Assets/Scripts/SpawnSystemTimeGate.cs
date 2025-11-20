using System.Linq;
using UnityEngine;

public class SpawnSystemTimeGate : MonoBehaviour
{
    [Header("Timer (ScoreAndCountdown só p/ referência de UI)")]
    public ScoreAndCountdownTMP timer;   // agora é só referência visual, lógica vem do GameTimer

    [Header("Spawners que serão controlados (auto-preenchido no Reset)")]
    public NPCSpawner[] spawners;

    [Header("Opções")]
    public bool disableSpawnersWhenTimeEnds = true;   // desabilita o componente ao zerar
    public bool killRemainingCustomersOnTimeUp = false;

    bool lastAllow = true;

    void Reset()
    {
        // tenta achar automaticamente os spawners
        spawners = GetComponentsInChildren<NPCSpawner>(true);

        // (opcional) tentar achar o ScoreAndCountdownTMP de um objeto "Score"
        if (!timer)
        {
            var scoreObj = GameObject.Find("Score");
            if (scoreObj) timer = scoreObj.GetComponent<ScoreAndCountdownTMP>();
        }
    }

    void Awake()
    {
        if (spawners == null || spawners.Length == 0)
            spawners = GetComponentsInChildren<NPCSpawner>(true);
    }

    void Update()
    {
        // 🕐 Se existir GameTimer, ele é a fonte oficial de tempo.
        // Se não existir, deixa os spawns liberados.
        bool allow = true;

        if (GameTimer.I != null)
            allow = GameTimer.I.Running;

        // nada mudou desde o último frame → não faz nada
        if (allow == lastAllow) return;
        lastAllow = allow;

        // liga/desliga spawners
        if (disableSpawnersWhenTimeEnds)
        {
            foreach (var sp in spawners.Where(x => x))
                sp.enabled = allow;
        }

        // opcional: limpa clientes quando o tempo zera
        if (!allow && killRemainingCustomersOnTimeUp)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            foreach (var c in FindObjectsByType<CustomerAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Destroy(c.gameObject);
#else
            foreach (var c in FindObjectsOfType<CustomerAI>())
                Destroy(c.gameObject);
#endif
        }
    }
}
