using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CustomerAI))]
[RequireComponent(typeof(CustomerOrder))]
public class CustomerTableWatcher : MonoBehaviour
{
    [Tooltip("Raio de busca pelo TableSpot ao SENTAR")]
    public float searchRadius = 1.2f;

    CustomerAI ai;
    CustomerOrder order;
    TableSpot watched;

    void Awake()
    {
        ai    = GetComponent<CustomerAI>();
        order = GetComponent<CustomerOrder>();
    }

    void OnEnable()
    {
        ai.OnSatDown += HandleSatDown;   // chamado pelo CustomerAI quando senta
    }

    void OnDisable()
    {
        ai.OnSatDown -= HandleSatDown;
        Unwatch();
    }

    void HandleSatDown(CustomerAI _)
    {
        // pega o TableSpot mais próximo (dentro do raio)
        watched = FindClosestSpot(transform.position, searchRadius);
        if (!watched) return;

        // escuta eventos da mesa
        watched.OnPlaced  += OnSpotPlaced;
        watched.OnCleared += OnSpotCleared;

        // se já tiver algo na mesa, valida imediatamente
        if (watched.isOccupied && watched.placedObject)
            TryConsumeIfMatches(watched, watched.placedObject);
    }

    void OnSpotPlaced(TableSpot spot, GameObject obj)
    {
        if (spot != watched) return;
        TryConsumeIfMatches(spot, obj);
    }

    void OnSpotCleared(TableSpot spot)
    {
        // nada obrigatório; mantemos o watch pra próximos pratos
    }

    void TryConsumeIfMatches(TableSpot spot, GameObject obj)
    {
        if (!ai.CanReceiveDish()) return;

        var cook = obj ? obj.GetComponent<Cookable>() : null;
        if (!cook) return;

        if (order != null && order.Matches(cook))
        {
            // começa o ato de comer; prato permanece no TableSpot
            ai.OnDishDeliveredFromTable(spot, cook);
        }
        else
        {
            // opcional: feedback de prato errado
            // Debug.Log("Prato não corresponde ao pedido.");
        }
    }

    void Unwatch()
    {
        if (watched != null)
        {
            watched.OnPlaced  -= OnSpotPlaced;
            watched.OnCleared -= OnSpotCleared;
            watched = null;
        }
    }

    static TableSpot FindClosestSpot(Vector3 pos, float radius)
    {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        var all = FindObjectsByType<TableSpot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var all = FindObjectsOfType<TableSpot>();
#endif
        TableSpot best = null;
        float bestD2 = radius * radius;

        foreach (var s in all)
        {
            float d2 = ((Vector2)pos - (Vector2)s.transform.position).sqrMagnitude;
            if (d2 <= bestD2)
            {
                bestD2 = d2;
                best   = s;
            }
        }
        return best;
    }
}
