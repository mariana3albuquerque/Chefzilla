using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D), typeof(NavMeshAgent))]
public class NavMeshAgent2DDriver : MonoBehaviour
{
    [Header("Camadas sólidas (Walls, Decor)")]
    public LayerMask obstacleMask;
    [Range(0.001f, 0.05f)] public float skin = 0.01f;

    [Header("Chegada")]
    [Range(0.06f, 0.3f)] public float arriveTol = 0.14f;

    Rigidbody2D rb;
    NavMeshAgent agent;
    Collider2D solidCol;
    ContactFilter2D filter;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];

    void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.updatePosition = false;

        // **Prefira CapsuleCollider2D como colisor sólido**
        var cols = GetComponents<Collider2D>().Where(c => !c.isTrigger).ToArray();
        solidCol = cols.OfType<CapsuleCollider2D>().FirstOrDefault() ?? cols.FirstOrDefault();

        filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(obstacleMask);
    }

    void FixedUpdate()
    {
        if (agent.isStopped || ReachedAgentGoal())
        {
            agent.ResetPath();
            agent.nextPosition = rb.position;
            return;
        }

        Vector2 vDes = agent.desiredVelocity;
        if (vDes.sqrMagnitude < 1e-5f) { agent.nextPosition = rb.position; return; }

        float maxStep = vDes.magnitude * Time.fixedDeltaTime;
        Vector2 dir   = vDes.normalized;

        if (solidCol && solidCol.Cast(dir, filter, hits, maxStep + skin) > 0)
        {
            // BLOQUEADO: pegue a colisão mais próxima
            var hit = hits[0];
            for (int i = 1; i < hits.Length && hits[i].collider; i++)
                if (hits[i].distance < hit.distance) hit = hits[i];

            Vector2 n = hit.normal;

            // Tangente candidata (pode degenerar em quina)
            Vector2 t = dir - Vector2.Dot(dir, n) * n;
            if (t.sqrMagnitude < 1e-6f) t = Vector2.Perpendicular(n); // fallback
            t.Normalize();

            // **Tente as DUAS tangentes e pegue a que tiver mais folga**
            float stepT1 = FreeStepAlong(t, maxStep);
            float stepT2 = FreeStepAlong(-t, maxStep);

            if (stepT1 > 0.0005f || stepT2 > 0.0005f)
            {
                bool useT1 = stepT1 >= stepT2;
                float step = useT1 ? stepT1 : stepT2;
                Vector2 tv = useT1 ? t : -t;
                rb.MovePosition(rb.position + tv * step);
            }
            else
            {
                // **Destravador de quina**: empurra um tiquinho para fora
                float unstick = Mathf.Min(0.03f, skin * 3f);
                rb.MovePosition(rb.position + n * unstick);
            }
        }
        else
        {
            rb.MovePosition(rb.position + dir * maxStep);
        }

        agent.nextPosition = rb.position;
        ClearHitsArray();
    }

    float FreeStepAlong(Vector2 slideDir, float maxStep)
    {
        float step = maxStep * 0.9f;
        int count = solidCol.Cast(slideDir, filter, hits, step + skin);
        if (count > 0)
        {
            float min = hits[0].distance;
            for (int i = 1; i < count && hits[i].collider; i++)
                if (hits[i].distance < min) min = hits[i].distance;
            step = Mathf.Max(0f, min - skin);
        }
        return step;
    }

    void ClearHitsArray()
    {
        for (int i = 0; i < hits.Length; i++) hits[i] = default;
    }

    bool ReachedAgentGoal()
    {
        if (agent.pathPending) return false;
        if (!agent.hasPath) return true;

        float tol = Mathf.Max(arriveTol, agent.stoppingDistance);
        if (agent.remainingDistance > tol) return false;

        Vector2 me  = rb.position;
        Vector2 end = agent.pathEndPosition;
        return Vector2.Distance(me, end) <= tol * 1.25f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!agent) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(agent.steeringTarget, 0.04f);
    }
#endif
}
