using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor; // gizmos/labels no editor
#endif

[AddComponentMenu("Nav2D/Seat Point")]
public class SeatPoint : MonoBehaviour
{
    [Header("Âncoras do Assento")]
    [Tooltip("Onde o NPC termina o path (no corredor, em cima do NavMesh).")]
    public Transform approach;

    [Tooltip("Ponto exato de 'sentar' (pode estar fora do NavMesh).")]
    public Transform anchor;

    [Header("Amostragem no NavMesh (segurança)")]
    [Range(0.05f, 0.8f)] public float sampleApproachRadius = 0.40f;
    [Range(0.05f, 0.8f)] public float sampleAnchorRadius   = 0.60f;

    // Gerência de ocupação / reservas
    private CustomerAI reservedBy;
    private CustomerAI occupiedBy;

    // Índice global para AIs escolherem assentos
    public static readonly List<SeatPoint> All = new();

    void OnEnable()
    {
        All.Add(this);
        AutoFindAnchors();
    }

    void OnDisable() => All.Remove(this);

    void OnValidate() => AutoFindAnchors();

    void AutoFindAnchors()
    {
        if (!approach) approach = transform.Find("Approach");
        if (!anchor)   anchor   = transform.Find("Anchor");
    }

    [ContextMenu("Create Approach & Anchor")]
    void CreateAnchorsContext()
    {
        if (!approach)
        {
            var a = new GameObject("Approach");
            a.transform.SetParent(transform, false);
            approach = a.transform;
        }
        if (!anchor)
        {
            var b = new GameObject("Anchor");
            b.transform.SetParent(transform, false);
            anchor = b.transform;
        }
    }

    // ---- Reserva / Ocupação -------------------------------------------------

    public bool IsFree => occupiedBy == null && reservedBy == null;

    public bool TryReserve(CustomerAI who)
    {
        if (!IsFree) return false;
        reservedBy = who;
        return true;
    }

    public void Occupy(CustomerAI who)
    {
        if (reservedBy == who || reservedBy == null)
        {
            occupiedBy = who;
            reservedBy = null;
        }
    }

    public void Vacate(CustomerAI who)
    {
        if (occupiedBy == who) occupiedBy = null;
        if (reservedBy == who) reservedBy = null;
    }

    // ---- Utilidades NavMesh --------------------------------------------------

    // Approach PRECISA estar no NavMesh
    public bool TryGetApproach(out Vector3 p)
    {
        var src = approach ? approach.position : transform.position;
        src.z = 0f;
        if (NavMesh.SamplePosition(src, out var hit, sampleApproachRadius, NavMesh.AllAreas))
        {
            p = hit.position; p.z = 0f; return true;
        }
        p = src; return false;
    }

    // Anchor pode estar fora do NavMesh; aqui só retorna a posição-alvo do sprite
    public bool TryGetAnchor(out Vector3 p)
    {
        p = (anchor ? anchor.position : transform.position);
        p.z = 0f;
        return true;
    }

    // ---- Gizmos (somente no Editor) -----------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 pApproach = approach ? approach.position : transform.position;
        Vector3 pAnchor   = anchor   ? anchor.position   : transform.position;

        float r = GetAgentRadiusApprox();

        // Approach = amarelo (navegação)
        DrawDisc(pApproach,
                 new Color(1f, 0.9f, 0.2f, 0.35f),
                 new Color(1f, 0.9f, 0.2f, 1f),
                 r);

        // Anchor = verde (posição do sprite)
        DrawDisc(pAnchor,
                 new Color(0.2f, 1f, 0.6f, 0.35f),
                 new Color(0.2f, 1f, 0.6f, 1f),
                 r);

        Handles.Label(pApproach + Vector3.up * 0.12f, "Approach");
        Handles.Label(pAnchor   + Vector3.up * 0.12f, "Anchor");
    }

    static void DrawDisc(Vector3 pos, Color fill, Color outline, float r)
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = fill;    Handles.DrawSolidDisc(pos, Vector3.forward, r);
        Handles.color = outline; Handles.DrawWireDisc (pos, Vector3.forward, r);

        float cross = r * 0.6f;
        Handles.DrawLine(pos + Vector3.right * cross, pos - Vector3.right * cross);
        Handles.DrawLine(pos + Vector3.up    * cross, pos - Vector3.up    * cross);
    }

    float GetAgentRadiusApprox()
    {
        var ag = GetComponentInChildren<NavMeshAgent>();
        if (ag) return Mathf.Max(0.08f, ag.radius);
        return 0.18f; // fallback razoável
    }
#endif
}
