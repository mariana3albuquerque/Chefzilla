using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    private NavMeshAgent agent;
    private TableController mesaAlvo;
    private bool chegouMesa = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject spawnObj = GameObject.Find("SpawnPoint");
        if (spawnObj != null)
        {
            Vector3 pos = spawnObj.transform.position;
            pos.z = 0f;
            transform.position = pos;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Debug.Log("NPC ajustado e alinhado ao NavMesh.");
            }
            else
            {
                Debug.LogWarning("SpawnPoint fora do NavMesh! NPC ficará travado.");
            }
        }
        else
        {
            Debug.LogWarning("SpawnPoint não encontrado na cena!");
        }

        BuscarMesaDisponivel();
    }

    void BuscarMesaDisponivel()
    {
        GameObject[] mesas = GameObject.FindGameObjectsWithTag("Mesa");
        Debug.Log("Encontradas " + mesas.Length + " mesas.");

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
                return;
            }
        }

        Debug.Log("Nenhuma mesa disponível no momento.");
    }

    void Update()
    {
        if (mesaAlvo != null && !chegouMesa)
        {
            Debug.DrawLine(transform.position, agent.destination, Color.red);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
                    Debug.LogWarning("NPC não conseguiu alcançar o destino (pathStatus=" + agent.pathStatus + ")");
                }
            }
        }
    }
}
