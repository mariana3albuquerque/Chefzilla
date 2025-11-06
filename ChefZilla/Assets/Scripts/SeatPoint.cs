using UnityEngine;

public class SeatPoint : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<SeatPoint> All = new();

    [Tooltip("Raio do gizmo (visual)")]
    public float radius = 0.15f;

    bool reserved, occupied;
    CustomerAI reserver, occupant;

    void OnEnable(){ if (!All.Contains(this)) All.Add(this); }
    void OnDisable(){ All.Remove(this); }

    public bool IsFree => !reserved && !occupied;

    public bool TryReserve(CustomerAI who){
        if (!IsFree) return false;
        reserved = true; reserver = who; return true;
    }
    public void Unreserve(CustomerAI who){
        if (reserver == who){ reserved = false; reserver = null; }
    }
    public void Occupy(CustomerAI who){
        reserved = false; reserver = null;
        occupied = true; occupant = who;
    }
    public void Vacate(CustomerAI who){
        if (occupant == who){ occupied = false; occupant = null; }
    }

#if UNITY_EDITOR
    void OnDrawGizmos(){
        Gizmos.color = IsFree ? new Color(0,1,0,0.6f) : new Color(1,0,0,0.6f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
