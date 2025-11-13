using System.Linq;
using UnityEngine;

public class SpawnSystemTimeGate : MonoBehaviour
{
    [Header("Timer (arraste o ScoreAndCountdownTMP do objeto 'Score')")]
    public ScoreAndCountdownTMP timer;

    [Header("Spawners que serão controlados (auto-preenchido no Reset)")]
    public NPCSpawner[] spawners;

    [Header("Opções")]
    public bool disableSpawnersWhenTimeEnds = true;   // desabilita o componente ao zerar
    public bool killRemainingCustomersOnTimeUp = false;

    bool lastAllow = true;

    void Reset()
    {
        // tenta achar automaticamente
        spawners = GetComponentsInChildren<NPCSpawner>(true);
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
        // se não houver timer, deixa spawns liberados
        bool allow = timer ? timer.IsRunning : true;

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
