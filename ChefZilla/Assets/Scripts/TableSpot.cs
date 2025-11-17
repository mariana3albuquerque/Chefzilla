using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TableSpot : MonoBehaviour
{
    [Header("Ligação com o assento (SeatPoint)")]
    public SeatPoint seat; // arraste o SeatPoint da mesa (ou deixamos achar no pai)

    [Header("Estado da Mesa")]
    public bool isOccupied = false;          // se já tem item
    public GameObject placedObject = null;   // referência ao item na mesa

    [Header("Indicador Visual (Hint Circle)")]
    public GameObject visualIndicator;

    void Awake()
    {
        // Se não foi setado no Inspector, tenta achar no pai
        if (!seat) seat = GetComponentInParent<SeatPoint>();

        // Mesa é ponto de interação: geralmente é melhor ser trigger
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger) col.isTrigger = true;
    }

    // Colocar um item na mesa
    public void Place(GameObject obj)
    {
        if (isOccupied || obj == null) return;

        // parent e snap
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // desativa física do item
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        // opcional: desativa collider do item para não atrapalhar
        var itemCol = obj.GetComponent<Collider2D>();
        if (itemCol) itemCol.enabled = false;

        placedObject = obj;
        isOccupied = true;

        // --- INTEGRAÇÃO COM CLIENTE: tentar servir se houver alguém sentado ---
        // Esperamos que o prato tenha DishItem (tipo do prato).
        var dish = obj.GetComponent<DishItem>();
        if (dish && seat && seat.CurrentOccupant != null)
        {
            var cust = seat.CurrentOccupant; // CustomerAI atual
            if (cust.TryServe(dish.type))
            {
                // prato correto: consumir e limpar a mesa
                Destroy(obj);      // ou obj.SetActive(false) se preferir não destruir
                Clear();
            }
            // se o prato for errado, não fazemos nada: ele permanece na mesa
        }
    }

    // Remover o item e devolver pro jogador
    public GameObject Remove()
    {
        if (!isOccupied || placedObject == null) return null;

        GameObject obj = placedObject;
        placedObject = null;
        isOccupied = false;

        // reativa física
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        // reativa collider do item
        var itemCol = obj.GetComponent<Collider2D>();
        if (itemCol) itemCol.enabled = true;

        // desapega do parent
        obj.transform.SetParent(null);
        return obj;
    }

    // limpar manualmente
    public void Clear()
    {
        placedObject = null;
        isOccupied = false;
    }

    // Ativa/desativa o círculo de dica
    public void SetHintActive(bool on)
    {
        if (visualIndicator != null)
            visualIndicator.SetActive(on);
    }
}
