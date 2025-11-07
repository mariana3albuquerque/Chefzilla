using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    [Header("Tempo sentado (s)")]
    public float minWait = 5f;
    public float maxWait = 12f;

    [Header("Checagem de chegada")]
    [Range(0.06f, 0.3f)] public float arriveTolerance = 0.14f;

    [Header("Fluxo de entrada/saída")]
    public Transform doorOutside;          // ponto lá fora (antes do teleporte)
    public bool mustEnterThroughDoor = true;
    public Transform exitPoint;            // ponto de saída

    NavMeshAgent agent;
    Rigidbody2D rb;
    SeatPoint seat;
    float waitTimer;

    enum State { GoingToDoor, GoingToApproach, Waiting, Leaving }
    State state;

    // watchdog de progresso (evita travar em cantos estreitos)
    Vector3 lastPos;
    float noProgressTimer;

    void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;   // NavMeshPlus 2D
        agent.updateUpAxis   = false;
        agent.updatePosition = false;   // quem move é o driver por Rigidbody2D

        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveTolerance);
        agent.autoRepath = true;
        agent.autoBraking = true;
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
        if (livres.Count == 0) { Invoke(nameof(ReserveSeatOrRetry), 1.0f); return; }

        // embaralha
        for (int i = 0; i < livres.Count; i++)
        {
            int j = Random.Range(i, livres.Count);
            (livres[i], livres[j]) = (livres[j], livres[i]);
        }

        foreach (var cand in livres)
        {
            // testa alcançabilidade do APPROACH ANTES de reservar
            if (!cand.TryGetApproach(out var approach)) continue;
            if (!TryBuildCompletePath(approach, out _)) continue;

            if (!cand.TryReserve(this)) continue; // corrida com outros clientes
            seat = cand;

            if (mustEnterThroughDoor && doorOutside != null)
            {
                GoTo(doorOutside.position);
                state = State.GoingToDoor;
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
        if (!seat) { ReserveSeatOrRetry(); return; }

        if (!seat.TryGetApproach(out var approach) || !TryBuildCompletePath(approach, out var path))
        {
            // assento inalcançável → libera e tenta outro
            seat.Vacate(this); seat = null;
            ReserveSeatOrRetry();
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveTolerance);
        agent.SetPath(path);

        lastPos = transform.position;
        noProgressTimer = 0f;
        state = State.GoingToApproach;
    }

    // chamado pelo teleporter DEPOIS de mover o NPC para o ponto interno
    public void OnTeleportedThroughDoor()
    {
        mustEnterThroughDoor = false;
        WarpBodyAndAgentToNavmesh(transform.position, 0.8f);  // garante que ficou em cima do azul
        agent.isStopped = false;
        GoToApproach();
    }

    // =========================================================
    // Loop
    // =========================================================
    void Update()
    {
        // vira o sprite conforme movimento do AGENT (não do Rigidbody)
        var sr = GetComponent<SpriteRenderer>();
        if (sr && Mathf.Abs(agent.desiredVelocity.x) > 0.01f)
            sr.flipX = agent.desiredVelocity.x < 0;

        switch (state)
        {
            case State.GoingToDoor:
                // fallback: se, por qualquer motivo, não teleportou e chegou na porta
                if (Arrived())
                    GoToApproach();
                break;

            case State.GoingToApproach:
                // watchdog de progresso
                float moved = (transform.position - lastPos).sqrMagnitude;
                if (moved < 0.0004f) noProgressTimer += Time.deltaTime;
                else { noProgressTimer = 0f; lastPos = transform.position; }

                // caminho estragou? replaneja
                if (agent.isPathStale || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    GoToApproach();
                    break;
                }

                // travou por tempo demais tentando contornar → escolhe outro assento
                if (noProgressTimer > 2.0f)
                {
                    if (seat) { seat.Vacate(this); seat = null; }
                    ReserveSeatOrRetry();
                    break;
                }

                if (Arrived())
                {
                    SitAtAnchor(); // sprite no anchor (pode ser fora do navmesh) + agent ancorado na malha
                    if (seat != null) seat.Occupy(this);

                    waitTimer = Random.Range(minWait, maxWait);
                    state = State.Waiting;
                }
                break;

            case State.Waiting:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    StartLeaving();
                }
                break;

            case State.Leaving:
                if (exitPoint && Arrived()) Destroy(gameObject);
                break;
        }
    }

    // =========================================================
    // Chegada / Sentar / Levantar
    // =========================================================
    bool Arrived()
    {
        if (agent.pathPending) return false;

        float tol = Mathf.Max(arriveTolerance, agent.stoppingDistance);
        if (agent.hasPath && agent.remainingDistance > tol) return false;

        var end = agent.pathEndPosition;
        end.z = 0f;
        if (Vector2.Distance((Vector2)transform.position, (Vector2)end) > tol * 1.25f) return false;

        return true;
    }

    void SitAtAnchor()
    {
        // 1) Sprite exatamente no Anchor (pode ser fora do navmesh)
        Vector3 anchorRaw = seat && seat.TryGetAnchor(out var aRaw) ? aRaw : transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = anchorRaw;

        // 2) Agent “gruda” no ponto válido mais próximo da malha
        Vector3 agentOnMesh = anchorRaw;
        if (NavMesh.SamplePosition(anchorRaw, out var hit, 0.6f, NavMesh.AllAreas))
            agentOnMesh = hit.position;

        agent.Warp(agentOnMesh);
        agent.ResetPath();
        agent.isStopped = true;

        // 3) Pausa o driver enquanto está sentado (mata jitter)
        var driver = GetComponent<NavMeshAgent2DDriver>();
        if (driver) driver.enabled = false;
    }

    void StartLeaving()
    {
        if (seat != null) seat.Vacate(this);

        // Alinha corpo+Agent em um ponto válido do NavMesh para sair:
        // usamos o APPROACH do assento como base (sempre on-mesh).
        Vector3 depart = transform.position;
        if (seat != null && seat.TryGetApproach(out var approachPos))
            depart = approachPos;

        WarpBodyAndAgentToNavmesh(depart, 0.8f);

        // reabilita driver e inicia saída
        var driver = GetComponent<NavMeshAgent2DDriver>();
        if (driver && !driver.enabled) driver.enabled = true;

        agent.isStopped = false;
        state = State.Leaving;

        if (exitPoint)
            GoTo(exitPoint.position);
        else
            Destroy(gameObject, 0.25f);
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
        // garante liberação do assento se destruir o cliente no meio do fluxo
        if (seat != null) { seat.Vacate(this); seat = null; }
    }
}
