using UnityEngine;

public class PlayerSimple : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 3.5f;

    [Header("Pegar / Interagir")]
    public Transform holdPoint;                 // arrastar Chef/HoldPoint aqui
    public float interactDistance = 0.8f;
    public float interactRadius = 0.35f;
    public LayerMask interactMask = ~0;         // Everything por padrão

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 facing = Vector2.down;
    private GameObject heldItem;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.parent = transform;
            hp.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            holdPoint = hp.transform;
        }
    }

    void Update()
    {
        // input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (moveInput != Vector2.zero) facing = moveInput;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = moveInput * moveSpeed;
    }

    void TryInteract()
    {
        Vector2 origin = (Vector2)transform.position + facing * interactDistance;
        Collider2D hit = Physics2D.OverlapCircle(origin, interactRadius, interactMask);

        if (hit == null) return;

        // se for uma estação (Fogao ou Bancada)
        StationSimple station = hit.GetComponent<StationSimple>();
        if (station != null)
        {
            station.InteractWithPlayer(this);
            return;
        }

        // se for um item no chão (tag "Item")
        if (hit.CompareTag("Item"))
        {
            Pickup(hit.gameObject);
            return;
        }
    }

    public void Pickup(GameObject item)
    {
        if (item == null || heldItem != null) return;

        // desativar colisor e rb para "pegar" item
        Collider2D col = item.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rbItem = item.GetComponent<Rigidbody2D>();
        if (rbItem != null) Destroy(rbItem);

        // torna filho do holdPoint
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        heldItem = item;

        // ajustar layer de desenho se tiver SpriteRenderer
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingLayerName = "Player";
    }

    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    public GameObject RemoveHeldItem()
    {
        GameObject temp = heldItem;
        heldItem = null;
        return temp;
    }

    void OnDrawGizmosSelected()
    {
        // desenha o círculo de interação na Scene View
        Gizmos.color = Color.cyan;
        Vector2 origin = (Vector2)transform.position + facing * interactDistance;
        Gizmos.DrawWireSphere(origin, interactRadius);
    }
}

