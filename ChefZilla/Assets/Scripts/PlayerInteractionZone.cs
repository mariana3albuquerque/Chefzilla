using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionZone : MonoBehaviour
{
    public Transform holdPoint;               // arraste aqui o HoldPoint do Chef
    public float tableSearchRadius = 1.0f;    // raio para procurar TableSpots
    public bool allowRAsAlias = false;        // (opcional) permitir R como alias para E

    [Header("Cooking")]
    [Tooltip("Tempo padrão se o fogão não tiver cookingTime > 0")]
    public float defaultCookTime = 2.5f;
    public bool blockMovementWhileCooking = true;

    Interactable currentInteractable = null;
    GameObject heldObject = null;

    // NOVO: referências do pai (Chef)
    Animator anim;
    PlayerController2D mover;

    bool isCooking = false;
    float cachedMoveSpeed = 0f;

    void Awake()
    {
        // pega no PAI (Chef)
        anim  = GetComponentInParent<Animator>();
        mover = GetComponentInParent<PlayerController2D>();
    }

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
        if (isCooking) return; // não responde E enquanto cozinha

        if (Input.GetKeyDown(KeyCode.E) || (allowRAsAlias && Input.GetKeyDown(KeyCode.R)))
        {
            // 1) Se estiver segurando algo -> tenta colocar na mesa livre mais próxima
            if (heldObject != null)
            {
                TableSpot targetSpot = FindNearestFreeTableSpot();
                if (targetSpot != null) PlaceOnTable(targetSpot);
                else Debug.Log("Não há vaga livre próxima para colocar o item.");
                return;
            }

            // 2) Sem item na mão e perto de um interactable
            if (currentInteractable != null)
            {
                if (currentInteractable.type == InteractableType.Stove)
                {
                    // NOVO: cozinha antes de pegar
                    StartCoroutine(CookThenPick(currentInteractable));
                    return;
                }

                if (currentInteractable.type == InteractableType.Fridge ||
                    currentInteractable.type == InteractableType.Table)
                {
                    PickFrom(currentInteractable);
                    return;
                }

                // fallback
                PickFrom(currentInteractable);
                return;
            }

            // 3) Tenta pegar de uma mesa ocupada próxima
            TableSpot occupied = FindNearestOccupiedTableSpot();
            if (occupied != null) PickFromTable(occupied);
            else Debug.Log("Nada para pegar por perto.");
        }
    }

    // ===================== COOKING =====================
    IEnumerator CookThenPick(Interactable stove)
    {
        if (stove == null || isCooking) yield break;
        isCooking = true;

        float cookTime = (stove.cookingTime > 0f) ? stove.cookingTime : defaultCookTime;

        // opcional: travar movimento enquanto cozinha
        if (blockMovementWhileCooking && mover != null)
        {
            cachedMoveSpeed = mover.moveSpeed;
            mover.moveSpeed = 0f;
        }

        if (anim) anim.SetBool("isCooking", true);

        float t = 0f;
        while (t < cookTime)
        {
            // se sair do trigger do fogão, cancela
            if (currentInteractable != stove) break;
            t += Time.deltaTime;
            yield return null;
        }

        if (anim) anim.SetBool("isCooking", false);
        if (blockMovementWhileCooking && mover != null) mover.moveSpeed = cachedMoveSpeed;

        // só pega se terminou e ainda está no fogão
        if (t >= cookTime && currentInteractable == stove)
            PickFrom(stove);

        isCooking = false;
    }
    // ===================================================

    void PickFrom(Interactable it)
    {
        if (it == null || it.spawnPrefab == null)
        {
            Debug.LogWarning("Interactable sem spawnPrefab: " + (it ? it.name : "null"));
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
            if (spot == null || spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist) { bestDist = d; best = spot; }
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
            if (spot == null || !spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist) { bestDist = d; best = spot; }
        }
        return best;
    }

    void PlaceOnTable(TableSpot spot)
    {
        if (heldObject == null || spot == null) return;
        heldObject.transform.SetParent(null);
        spot.Place(heldObject);
        heldObject = null;
    }

    public void DropHeldObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        var rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;
        heldObject = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tableSearchRadius);
    }
}
