using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class SeatArrivalWatcher : MonoBehaviour
{
    public NavMeshAgent agent;
    public float minStop = 0.33f;
    public float velThreshold = 0.001f;
    public UnityEvent onSeated;
    bool fired;

    void Awake() { if (!agent) agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (fired || !agent || agent.pathPending) return;
        float thresh = Mathf.Max(agent.stoppingDistance, minStop);
        if (agent.remainingDistance <= thresh && agent.velocity.sqrMagnitude <= velThreshold)
        {
            fired = true;
            onSeated?.Invoke();
        }
    }
}
