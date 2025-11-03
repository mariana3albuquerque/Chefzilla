using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    private NavMeshAgent agent;
    private TableController mesaAlvo;
    private bool indoEntrada = true;
    private bool entrouSalao = false;
    private bool chegouMesa = false;
    private bool mesaEscolhida = false;

    public Transform entradaExterna;
    public Transform entradaInterna;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject spawnObj = GameObject.Find("SpawnPoint");
        if (spawnObj != null)
            transform.position = spawnObj.transform.position;

        agent.SetDestination(entradaExterna.position);
    }

    void Update()
    {
        if (indoEntrada)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                indoEntrada = false;
                StartCoroutine(EntrarNoRestaurante());
            }
        }
        else if (entrouSalao && !chegouMesa && !mesaEscolhida)
        {
            BuscarMesaDisponivel();
        }
        else if (mesaAlvo != null && !chegouMesa)
        {
            float distanciaDestino = Vector3.Distance(transform.position, agent.destination);
            // Considera "chegou" se estiver suficientemente perto (ex: 0.2f)
            if (distanciaDestino < 0.2f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                chegouMesa = true;
                Debug.Log("NPC chegou suficientemente perto da mesa e parou.");
            }
            else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    chegouMesa = true;
                    Debug.Log("NPC chegou à mesa e parou.");
                }
                else
                {
                    // Só alerta se estiver realmente longe do destino
                    if (distanciaDestino > 0.5f)
                    {
                        Debug.LogWarning("NPC não conseguiu alcançar o destino (pathStatus=" + agent.pathStatus + ")");
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator EntrarNoRestaurante()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        agent.enabled = false;
        yield return new WaitForSeconds(0.15f);

        transform.position = entradaInterna.position;
        agent.enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        entrouSalao = true;
    }

    void BuscarMesaDisponivel()
    {
        GameObject[] mesas = GameObject.FindGameObjectsWithTag("Mesa");
        foreach (GameObject mesaObj in mesas)
        {
            TableController table = mesaObj.GetComponent<TableController>();
            if (table != null && table.EstaDisponivel())
            {
                mesaAlvo = table;
                mesaAlvo.Ocupar();
                Vector3 destinoOriginal = mesaAlvo.pontoDeAssento.position;
                destinoOriginal.z = 0f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(destinoOriginal, out hit, 1f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    Debug.Log("Destino do NPC: " + destinoOriginal + " (ajustado para NavMesh: " + hit.position + ")");
                }
                else
                {
                    Debug.LogWarning("pontoDeAssento NÃO está sobre o NavMesh! NPC não irá.");
                }
                mesaEscolhida = true;
                return;
            }
        }
        Debug.Log("Nenhuma mesa disponível no momento.");
    }
}
