using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] Transform destination;
    [SerializeField] string requiredTag = "";   // ex.: "Customer"
    [SerializeField] float sampleRadius = 1.5f;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var agent = other.GetComponent<NavMeshAgent>();
        if (!agent || !destination) return;

        var ai = other.GetComponent<CustomerAI>();
        StartCoroutine(TeleportThenNotify(agent, destination.position, ai));
    }

    IEnumerator TeleportThenNotify(NavMeshAgent agent, Vector3 to, CustomerAI ai)
    {
        agent.isStopped = true;                  // pausa antes do warp
        yield return null;                       // 1 frame

        if (NavMesh.SamplePosition(to, out var hit, sampleRadius, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            agent.Warp(to);

        yield return null;                       // dá tempo do agent “grudar” no NavMesh
        agent.isStopped = false;                 // garante que voltou a andar

        if (ai) ai.OnTeleportedThroughDoor();    // **AVISA DEPOIS DO WARP**
    }
}
