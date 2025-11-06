using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class CustomerAI : MonoBehaviour
{
    [Header("Tempo sentado")]
    public float minWait = 5f;
    public float maxWait = 12f;

    [Header("Checagem de chegada")]
    public float arriveTolerance = 0.08f;

    [Header("Fluxo de entrada/saída")]
    public Transform doorOutside;
    public bool mustEnterThroughDoor = true;
    public Transform exitPoint;

    NavMeshAgent agent;
    SeatPoint seat;
    float waitTimer;

    enum State { GoingToDoor, SeekingSeat, Waiting, Leaving }
    State state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;   // NavMeshPlus 2D
        agent.updateUpAxis   = false;
    }

    void Start()
    {
        // Garante que começa sobre a malha
        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        ReserveSeatOrRetry();
    }

    // ======== fluxo de reserva e ida ========
    void ReserveSeatOrRetry()
    {
        var free = SeatPoint.All.Where(s => s.IsFree).ToList();
        if (free.Count == 0) { Invoke(nameof(ReserveSeatOrRetry), 1.5f); return; }

        for (int tries = 0; tries < free.Count; tries++)
        {
            var cand = free[Random.Range(0, free.Count)];
            if (!cand.TryReserve(this)) continue;

            seat = cand;

            if (mustEnterThroughDoor && doorOutside != null)
            {
                GoTo(doorOutside.position);     // <— agora existe
                state = State.GoingToDoor;
            }
            else
            {
                GoToSeat();
            }
            return;
        }

        Invoke(nameof(ReserveSeatOrRetry), 1f);
    }

    void GoToSeat()
    {
        if (!seat) { ReserveSeatOrRetry(); return; }

        agent.isStopped = false;
        if (!SetSafeDestination(seat.transform.position))
        {
            // Fallback: tenta um pequeno deslocamento se a borda da malha atrapalhar
            Vector3 p = seat.transform.position + new Vector3(0.08f, 0, 0);
            if (!SetSafeDestination(p))
                Invoke(nameof(GoToSeat), 0.2f);
        }

        state = State.SeekingSeat;
    }

    // chamado pelo teleporter DEPOIS do warp
    public void OnTeleportedThroughDoor()
    {
        mustEnterThroughDoor = false;   // não volte para a porta
        agent.isStopped = false;
        GoToSeat();
    }

    void Update()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr && Mathf.Abs(agent.velocity.x) > 0.01f)
            sr.flipX = agent.velocity.x < 0;

        switch (state)
        {
            case State.GoingToDoor:
                // fallback se por algum motivo não teleportar
                if (Arrived()) GoToSeat();
                break;

            case State.SeekingSeat:
                if (Arrived())
                {
                    if (seat != null) seat.Occupy(this);
                    waitTimer = Random.Range(minWait, maxWait);
                    agent.isStopped = true;
                    state = State.Waiting;
                }
                break;

            case State.Waiting:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    if (seat != null) seat.Vacate(this);
                    agent.isStopped = false;
                    state = State.Leaving;

                    if (exitPoint) SetSafeDestination(exitPoint.position);
                    else Destroy(gameObject, 0.25f);
                }
                break;

            case State.Leaving:
                if (exitPoint && Arrived()) Destroy(gameObject);
                break;
        }
    }

    bool Arrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > Mathf.Max(arriveTolerance, agent.stoppingDistance)) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.0001f) return false;
        return true;
    }

    // ——— utilitários de navegação ———
    bool SetSafeDestination(Vector3 p)
    {
        p.z = 0f;
        var path = new NavMeshPath();
        if (agent.CalculatePath(p, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(p);
            return true;
        }
        return false;
    }

    // Wrapper para manter compatibilidade com chamadas antigas
    void GoTo(Vector3 p)
    {
        agent.isStopped = false;
        if (!SetSafeDestination(p))
            agent.SetDestination(p); // tenta mesmo assim; teleporte pode resolver
    }
}
