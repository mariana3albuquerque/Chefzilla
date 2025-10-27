using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionZone : MonoBehaviour
{
    public Transform holdPoint;               // arraste aqui o HoldPoint do Chef
    public float tableSearchRadius = 1.0f;    // raio para procurar TableSpots
    public bool allowRAsAlias = false;        // (opcional) permitir R como alias para E

    Interactable currentInteractable = null;
    GameObject heldObject = null;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it != null) currentInteractable = it;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it != null && it == currentInteractable) currentInteractable = null;
    }

    void Update()
    {
        // Responde a E (ou R como alias, se allowRAsAlias = true)
        if (Input.GetKeyDown(KeyCode.E) || (allowRAsAlias && Input.GetKeyDown(KeyCode.R)))
        {
            // 1) Se estiver segurando algo -> tenta colocar na mesa livre mais próxima
            if (heldObject != null)
            {
                TableSpot targetSpot = FindNearestFreeTableSpot();
                if (targetSpot != null)
                {
                    PlaceOnTable(targetSpot);
                }
                else
                {
                    Debug.Log("Não há vaga livre próxima para colocar o item.");
                }
                return;
            }

            // 2) Se não estiver segurando -> primeiro tenta pegar de um interactable (fogão/geladeira)
            if (heldObject == null && currentInteractable != null)
            {
                if (currentInteractable.type == InteractableType.Stove ||
                    currentInteractable.type == InteractableType.Fridge)
                {
                    PickFrom(currentInteractable);
                }
                return;
            }

            // 3) Se não segurando e não tem interactable próximo -> tenta pegar de uma TableSpot ocupada (mais próxima)
            if (heldObject == null)
            {
                TableSpot occupied = FindNearestOccupiedTableSpot();
                if (occupied != null)
                {
                    PickFromTable(occupied);
                }
                else
                {
                    Debug.Log("Nada para pegar por perto.");
                }
                return;
            }
        }
    }

    void PickFrom(Interactable it)
    {
        if (it.spawnPrefab == null)
        {
            Debug.LogWarning("Interactable sem spawnPrefab: " + it.name);
            return;
        }

        heldObject = Instantiate(it.spawnPrefab, holdPoint.position, Quaternion.identity);
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;

        var rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
    }

    // Pegar o objeto que está na mesa (TableSpot.Remove())
    void PickFromTable(TableSpot spot)
    {
        if (spot == null) return;

        GameObject obj = spot.Remove();
        if (obj == null)
        {
            Debug.Log("TableSpot estava vazio quando tentou pegar.");
            return;
        }

        // parenta o objeto na mão e desativa física
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        heldObject = obj;
    }

    TableSpot FindNearestFreeTableSpot()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var spot = c.GetComponent<TableSpot>();
            if (spot == null) continue;
            if (spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = spot;
            }
        }

        return best;
    }

    TableSpot FindNearestOccupiedTableSpot()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var spot = c.GetComponent<TableSpot>();
            if (spot == null) continue;
            if (!spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = spot;
            }
        }

        return best;
    }

    void PlaceOnTable(TableSpot spot)
    {
        if (heldObject == null || spot == null) return;

        // Remove do holdPoint (desparenta), e coloca via spot.Place
        // Antes reparenta para world (tirar do hold) para evitar casos estranhos
        heldObject.transform.SetParent(null);
        spot.Place(heldObject);
        heldObject = null;
    }

    // debug/forçar largar
    public void DropHeldObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        var rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;
        heldObject = null;
    }

    // Gizmo para ver o raio no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tableSearchRadius);
    }
}

