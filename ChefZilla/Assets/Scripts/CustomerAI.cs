using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D), typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    [Header("Tempo sentado (s)")]
    public float minWait = 5f;
    public float maxWait = 12f;

    [Header("Tempo comendo (fallback, se o prato não tiver eatTime)")]
    public float minEat = 4f;
    public float maxEat = 7f;

    [Header("Checagem de chegada")]
    [Range(0.06f, 0.3f)] public float arriveTolerance = 0.14f;

    [Header("Fluxo de entrada/saída")]
    public Transform doorOutside;          // ponto lá fora (antes do teleporte)
    public bool mustEnterThroughDoor = true;
    public Transform exitPoint;            // ponto final de saída
    public Transform exitPivotTop;         // pivot corredor superior
    public Transform exitPivotBottom;      // pivot corredor inferior

    [Header("Segurança de movimentação")]
    [Tooltip("Tempo máximo tentando ir até o APPROACH antes de forçar o 'sentar' (fallback).")]
    public float maxTimeGoingToApproach = 6f;

    // runtime
    NavMeshAgent agent;
    Rigidbody2D rb;
    SeatPoint seat;
    Animator anim;
    SpriteRenderer sr;

    float waitTimer;

    // Comer / pontuação
    float eatTimer;
    Cookable servedDish;
    TableSpot eatingFromSpot;

    // máquina de estados
    enum State { GoingToDoor, GoingToApproach, Waiting, Eating, Leaving }
    State state;
    [SerializeField, Tooltip("Só pra debug no Inspector")]
    State debugState;

    void SetState(State s)
    {
        state = s;
        debugState = s;
    }

    // humor ao sair
    enum LeaveMood { None, Satisfied, Angry }
    LeaveMood leaveMood = LeaveMood.None;

    // watchdog de progresso
    Vector3 lastPos;
    float noProgressTimer;

    // saída via pivots
    Transform currentExitPivot;
    bool goingToExitPoint;

    // tempo no estado GoingToApproach (pra timeout)
    float goingToApproachTimer;

    // dispara quando o cliente efetivamente "senta"
    public event Action<CustomerAI> OnSatDown;

    void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponent<Animator>();
        sr    = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.updatePosition = false;

        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveTolerance);
        agent.autoRepath = true;
        agent.autoBraking = true;

        // se o valor no prefab estiver 0 (novo campo), define um padrão razoável
        if (maxTimeGoingToApproach <= 0.05f)
            maxTimeGoingToApproach = 6f;

        // nenhuma animação até sentar
        if (anim != null)
            anim.enabled = false;

        SetState(State.GoingToApproach);
    }

    void Start()
    {
        // Garante início sobre a malha
        WarpBodyAndAgentToNavmesh(transform.position, 1.0f);
        ReserveSeatOrRetry();
    }

    // =========================================================
    // Reserva & ida ao APPROACH do assento
    // =========================================================
    void ReserveSeatOrRetry()
    {
        var livres = SeatPoint.All.Where(s => s.IsFree).ToList();
        if (livres.Count == 0)
        {
            Invoke(nameof(ReserveSeatOrRetry), 1.0f);
            return;
        }

        // embaralha
        for (int i = 0; i < livres.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, livres.Count);
            (livres[i], livres[j]) = (livres[j], livres[i]);
        }

        foreach (var cand in livres)
        {
            if (!cand.TryGetApproach(out var approach)) continue;
            if (!TryBuildCompletePath(approach, out _)) continue;

            if (!cand.TryReserve(this)) continue;
            seat = cand;

            if (mustEnterThroughDoor && doorOutside != null)
            {
                GoTo(doorOutside.position);
                SetState(State.GoingToDoor);
            }
            else
            {
                GoToApproach();
            }
            return;
        }

        // ninguém válido → tenta de novo em instantes
        Invoke(nameof(ReserveSeatOrRetry), 0.75f);
    }

    void GoToApproach()
    {
        if (!seat)
        {
            ReserveSeatOrRetry();
            return;
        }

        if (!seat.TryGetApproach(out var approach) || !TryBuildCompletePath(approach, out var path))
        {
            // assento inalcançável → libera e tenta outro
            seat.Vacate(this);
            seat = null;
            ReserveSeatOrRetry();
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveTolerance);
        agent.SetPath(path);

        lastPos = transform.position;
        noProgressTimer = 0f;
        goingToApproachTimer = 0f;

        SetState(State.GoingToApproach);
    }

    // chamado pelo teleporter depois de mover o NPC para dentro
    public void OnTeleportedThroughDoor()
    {
        mustEnterThroughDoor = false;
        WarpBodyAndAgentToNavmesh(transform.position, 0.8f);
        agent.isStopped = false;
        GoToApproach();
    }

    // =========================================================
    // Loop principal
    // =========================================================
    void Update()
    {
        // vira sprite conforme direção
        if (sr && Mathf.Abs(agent.desiredVelocity.x) > 0.01f)
            sr.flipX = agent.desiredVelocity.x < 0;

        switch (state)
        {
            case State.GoingToDoor:
                if (Arrived())
                    GoToApproach();
                break;

            case State.GoingToApproach:
                UpdateGoingToApproach();
                break;

            case State.Waiting:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    leaveMood = LeaveMood.Angry;
                    StartLeaving();
                }
                break;

            case State.Eating:
                UpdateEating();
                break;

            case State.Leaving:
                UpdateLeaving();
                break;
        }

        UpdateAnimator();
    }

    void UpdateGoingToApproach()
    {
        // tempo total tentando chegar à mesa
        goingToApproachTimer += Time.deltaTime;

        // watchdog de progresso
        float moved = (transform.position - lastPos).sqrMagnitude;
        if (moved < 0.0004f) noProgressTimer += Time.deltaTime;
        else { noProgressTimer = 0f; lastPos = transform.position; }

        // caminho estragou? replaneja
        if (agent.isPathStale || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            GoToApproach();
            return;
        }

        // travou por tempo demais contornando -> troca de assento
        if (noProgressTimer > 2.0f)
        {
            if (seat) { seat.Vacate(this); seat = null; }
            ReserveSeatOrRetry();
            return;
        }

        // timeout de segurança (só se valor > 0)
        if (maxTimeGoingToApproach > 0f &&
            goingToApproachTimer > maxTimeGoingToApproach)
        {
            Debug.LogWarning(
                $"[CustomerAI] {name} demorou demais para chegar ao APPROACH, forçando SitAtAnchor()."
            );
            ForceSitAndWait();
            return;
        }

        if (Arrived())
        {
            SitAtAnchor();
            if (seat != null) seat.Occupy(this);

            waitTimer = UnityEngine.Random.Range(minWait, maxWait);
            leaveMood = LeaveMood.None;
            SetState(State.Waiting);
        }
    }

    void UpdateEating()
    {
        eatTimer -= Time.deltaTime;
        if (eatTimer > 0f) return;

        if (servedDish)
        {
            ScoreManager.I?.Add(servedDish.GetPoints());

            if (CurrencyManager.I != null)
            {
                int coins = servedDish.GetCoinsReward();
                CurrencyManager.I.AddCoins(coins);
            }

            if (eatingFromSpot)
            {
                if (eatingFromSpot.placedObject)
                    Destroy(eatingFromSpot.placedObject);
                eatingFromSpot.Clear();
                eatingFromSpot = null;
            }

            servedDish = null;
        }

        leaveMood = LeaveMood.Satisfied;
        StartLeaving();
    }

    // =========================================================
    // Chegada / sentar / levantar
    // =========================================================
    bool Arrived()
    {
        if (agent.pathPending) return false;

        float tol = Mathf.Max(arriveTolerance, agent.stoppingDistance);

        // se não tem path, considera que ainda não chegou (evita teleporte precoce)
        if (!agent.hasPath) return false;

        // critério principal
        if (agent.remainingDistance <= tol)
            return true;

        // fallback: distância até o fim do path
        Vector2 me  = rb ? rb.position : (Vector2)transform.position;
        Vector2 end = agent.pathEndPosition;
        float distToEnd = Vector2.Distance(me, end);

        if (distToEnd <= tol * 1.5f)
            return true;

        return false;
    }

    void SitAtAnchor()
    {
        Vector3 anchorRaw = seat && seat.TryGetAnchor(out var aRaw) ? aRaw : transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = anchorRaw;

        Vector3 agentOnMesh = anchorRaw;
        if (NavMesh.SamplePosition(anchorRaw, out var hit, 0.6f, NavMesh.AllAreas))
            agentOnMesh = hit.position;

        agent.Warp(agentOnMesh);
        agent.ResetPath();
        agent.isStopped = true;

        var driver = GetComponent<NavMeshAgent2DDriver>();
        if (driver) driver.enabled = false;

        OnSatDown?.Invoke(this);

        if (anim != null && !anim.enabled)
            anim.enabled = true;
    }

    void ForceSitAndWait()
    {
        SitAtAnchor();
        if (seat != null) seat.Occupy(this);

        waitTimer = UnityEngine.Random.Range(minWait, maxWait);
        leaveMood = LeaveMood.None;
        SetState(State.Waiting);
    }

    void StartLeaving()
    {
        var order = GetComponent<CustomerOrder>();
        if (order) order.LimparPedido();

        if (seat != null) seat.Vacate(this);

        Vector3 depart = transform.position;
        if (seat != null && seat.TryGetApproach(out var approachPos))
            depart = approachPos;

        WarpBodyAndAgentToNavmesh(depart, 0.8f);
        var driver = GetComponent<NavMeshAgent2DDriver>();
        if (driver && !driver.enabled) driver.enabled = true;

        agent.isStopped = false;

        currentExitPivot = GetBestExitPivot(depart);
        goingToExitPoint = false;

        SetState(State.Leaving);

        if (currentExitPivot != null)
        {
            GoTo(currentExitPivot.position);    // mesa -> pivot
        }
        else if (exitPoint != null)
        {
            GoTo(exitPoint.position);           // sem pivot: direto
            goingToExitPoint = true;
        }
        else
        {
            Destroy(gameObject, 0.25f);
        }
    }

    void UpdateLeaving()
    {
        if (exitPoint == null && currentExitPivot == null)
        {
            if (Arrived()) Destroy(gameObject);
            return;
        }

        if (!goingToExitPoint)
        {
            if (Arrived())
            {
                if (exitPoint != null)
                {
                    GoTo(exitPoint.position);
                    goingToExitPoint = true;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            return;
        }

        if (exitPoint != null && Arrived())
        {
            Destroy(gameObject);
        }
    }

    Transform GetBestExitPivot(Vector3 from)
    {
        Transform best = null;
        float bestDistSq = float.MaxValue;

        void TryCandidate(Transform t)
        {
            if (!t) return;
            Vector2 a = new Vector2(from.x, from.y);
            Vector2 b = new Vector2(t.position.x, t.position.y);
            float d2 = (a - b).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                best = t;
            }
        }

        TryCandidate(exitPivotTop);
        TryCandidate(exitPivotBottom);

        return best;
    }

    // =========================================================
    // Utilidades de navegação
    // =========================================================
    void GoTo(Vector3 p)
    {
        var target = SampleOnNavmesh(p, 0.8f, out bool ok);
        if (ok && TryBuildCompletePath(target, out var path))
        {
            agent.isStopped = false;
            agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveTolerance);
            agent.SetPath(path);
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(p);
        }
    }

    bool TryBuildCompletePath(Vector3 p, out NavMeshPath path)
    {
        path = new NavMeshPath();
        var target = SampleOnNavmesh(p, 0.4f, out bool ok);
        if (!ok) return false;
        if (!agent.CalculatePath(target, path)) return false;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    Vector3 SampleOnNavmesh(Vector3 pos, float maxDist, out bool ok)
    {
        pos.z = 0f;
        if (NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas))
        {
            ok = true;
            return hit.position;
        }
        ok = false;
        return pos;
    }

    void WarpBodyAndAgentToNavmesh(Vector3 pos, float maxDist)
    {
        pos.z = 0f;
        Vector3 p = pos;
        if (NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas))
            p = hit.position;

        rb.position = p;
        agent.Warp(p);
    }

    void OnDestroy()
    {
        if (seat != null) { seat.Vacate(this); seat = null; }
    }

    // =========================================================
    // Interface com a mesa
    // =========================================================
    public bool CanReceiveDish() => state == State.Waiting && seat != null;

    public void OnDishDeliveredFromTable(TableSpot spot, Cookable dish)
    {
        if (!CanReceiveDish() || !spot || !dish) return;

        var order = GetComponent<CustomerOrder>();
        if (order) order.LimparPedido();

        eatingFromSpot = spot;
        servedDish     = dish;

        if (eatingFromSpot.placedObject)
            eatingFromSpot.placedObject.SetActive(false);

        eatTimer = dish.eatTime > 0f
            ? dish.eatTime
            : UnityEngine.Random.Range(minEat, maxEat);

        SetState(State.Eating);
        UpdateAnimator();
    }

    // =========================================================
    // Animação
    // =========================================================
    void UpdateAnimator()
    {
        if (!anim || !anim.enabled) return;

        bool isEating       = (state == State.Eating);
        bool isLeavingHappy = (state == State.Leaving && leaveMood == LeaveMood.Satisfied);
        bool isLeavingAngry = (state == State.Leaving && leaveMood == LeaveMood.Angry);

        anim.SetBool("IsEating",    isEating);
        anim.SetBool("IsSatisfied", isLeavingHappy);
        anim.SetBool("IsAngry",     isLeavingAngry);
        // quando os três forem false, cai no "waiting" default
    }
}
