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
            BuscarMesaDisponivelMaisProxima();
        }
        else if (mesaAlvo != null && !chegouMesa)
        {
            float distanciaDestino = Vector3.Distance(transform.position, agent.destination);
            // Considere "chegou" se estiver suficientemente perto (ex: 0.2f)
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
                    if (distanciaDestino > 0.9f)
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

        // Força Z=0 na interna!
        transform.position = new Vector3(entradaInterna.position.x, entradaInterna.position.y, 0f);
        agent.enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        entrouSalao = true;
    }

    // Escolhe a mesa disponível mais próxima!
    void BuscarMesaDisponivelMaisProxima()
    {
        GameObject[] mesas = GameObject.FindGameObjectsWithTag("Mesa");
        TableController mesaMaisProxima = null;
        float menorDistancia = Mathf.Infinity;
        Vector3 origem = transform.position;

        foreach (GameObject mesaObj in mesas)
        {
            TableController table = mesaObj.GetComponent<TableController>();
            if (table != null && table.EstaDisponivel())
            {
                Vector3 pontoAssento = table.pontoDeAssento.position;
                pontoAssento.z = 0f;
                float dist = Vector3.Distance(origem, pontoAssento);

                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    mesaMaisProxima = table;
                }
            }
        }

        if (mesaMaisProxima != null)
        {
            mesaAlvo = mesaMaisProxima;
            mesaAlvo.Ocupar();
            Vector3 destinoOriginal = mesaAlvo.pontoDeAssento.position;
            destinoOriginal.z = 0f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destinoOriginal, out hit, 1f, NavMesh.AllAreas))
            {
                Vector3 destinoNavMesh = hit.position;
                destinoNavMesh.z = 0f; // Garante Z=0 sempre!
                agent.SetDestination(destinoNavMesh);
                Debug.Log("Destino do NPC: " + destinoOriginal + " (ajustado para NavMesh: " + destinoNavMesh + ")");
            }
            else
            {
                Debug.LogWarning("pontoDeAssento NÃO está sobre o NavMesh! NPC não irá.");
            }
            mesaEscolhida = true;
            return;
        }
        Debug.Log("Nenhuma mesa disponível no momento.");
    }
}
