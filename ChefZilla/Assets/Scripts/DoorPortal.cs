using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("Nav2D/Door Portal")]
public class DoorPortal : MonoBehaviour
{
    public Transform outside;   // âncora do lado de fora
    public Transform inside;    // âncora do lado de dentro

    public static readonly List<DoorPortal> All = new List<DoorPortal>();

    void OnEnable()
    {
        All.Add(this);
        AutoFindAnchors();
    }
    void OnDisable() => All.Remove(this);

    void OnValidate() => AutoFindAnchors();

    void AutoFindAnchors()
    {
        if (!outside) outside = transform.Find("Outside");
        if (!inside)  inside  = transform.Find("Inside");
    }

    // Retorna o portal cujo "outside" é mais próximo de 'pos'
    public static DoorPortal FindClosestOutside(Vector3 pos)
    {
        DoorPortal best = null;
        float bestD = float.PositiveInfinity;

        foreach (var p in All)
        {
            if (!p || !p.outside) continue;
            float d = (p.outside.position - pos).sqrMagnitude;
            if (d < bestD) { bestD = d; best = p; }
        }
        return best;
    }

    public void TeleportIn(GameObject go)
    {
        if (!inside) return;
        Vector3 p = SampleOnNavmeshOrFallback(inside.position, 0.8f);

        // sincroniza transform, Rigidbody2D e NavMeshAgent (se houver)
        var rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d) rb2d.position = p; else go.transform.position = p;

        var ag = go.GetComponent<NavMeshAgent>();
        if (ag) ag.Warp(p);

        // avisa o AI (se existir)
        var ai = go.GetComponent<CustomerAI>();
        if (ai) ai.OnTeleportedThroughDoor();
    }

    public void TeleportOut(GameObject go)
    {
        if (!outside) return;
        Vector3 p = SampleOnNavmeshOrFallback(outside.position, 0.8f);

        var rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d) rb2d.position = p; else go.transform.position = p;

        var ag = go.GetComponent<NavMeshAgent>();
        if (ag) ag.Warp(p);
    }

    static Vector3 SampleOnNavmeshOrFallback(Vector3 pos, float maxDist)
    {
        pos.z = 0f;
        if (NavMesh.SamplePosition(pos, out var hit, maxDist, NavMesh.AllAreas))
            return hit.position;
        return pos;
    }

    // Conveniência no menu de contexto
    [ContextMenu("Create Anchors (Outside/Inside)")]
    void CreateAnchorsContext()
    {
        if (!outside)
        {
            var o = new GameObject("Outside");
            o.transform.SetParent(transform, false);
            outside = o.transform;
        }
        if (!inside)
        {
            var i = new GameObject("Inside");
            i.transform.SetParent(transform, false);
            inside = i.transform;
        }
    }
}
