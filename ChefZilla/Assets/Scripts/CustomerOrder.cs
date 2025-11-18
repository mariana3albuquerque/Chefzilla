using UnityEngine;

[System.Serializable]
public struct DishOption
{
    public string id;     // ex: "soup", "burger"
    public Sprite icon;   // ícone para a bolha
}

public class CustomerOrder : MonoBehaviour
{
    public OrderBubble bubble;
    [Tooltip("Lista de pratos possíveis para este cliente")]
    public DishOption[] options;
    [Tooltip("Atraso para aparecer a bolha DEPOIS de sentar")]
    public float atrasoDepoisDeSentar = 1.5f;

    public string orderedDishId { get; private set; }
    public Sprite itemAtual     { get; private set; }

    CustomerAI customerAI;
    bool hasOrder;  // já sorteou um prato pra esse cliente?

    void Awake()
    {
        customerAI = GetComponent<CustomerAI>();
    }

    void OnEnable()
    {
        // quando o cliente sentar, chamamos HandleSatDown
        if (customerAI != null)
            customerAI.OnSatDown += HandleSatDown;
    }

    void OnDisable()
    {
        if (customerAI != null)
            customerAI.OnSatDown -= HandleSatDown;
    }

    void OnDestroy()
    {
        if (customerAI != null)
            customerAI.OnSatDown -= HandleSatDown;
    }

    /// <summary>
    /// Pede para esse cliente fazer um pedido.
    /// Se ele ainda não sentou, a bolha só aparecerá depois do OnSatDown.
    /// </summary>
    public void SolicitarPedido()
    {
        // evita criar vários pedidos pro mesmo cliente
        if (hasOrder) return;

        if (!bubble)
            bubble = GetComponentInChildren<OrderBubble>(true);

        if (!bubble)
        {
            Debug.LogWarning("CustomerOrder: sem OrderBubble configurado.");
            return;
        }

        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("CustomerOrder: sem opções de prato configuradas.");
            return;
        }

        // sorteia o prato
        var opt = options[Random.Range(0, options.Length)];
        orderedDishId = opt.id;
        itemAtual     = opt.icon;
        hasOrder      = true;

        // Se já estiver sentado, mostra a bolha agora mesmo (com atraso interno)
        if (customerAI == null || customerAI.HasSatDown)
        {
            bubble.Show(itemAtual, atrasoDepoisDeSentar);
        }
        else
        {
            // Ainda não sentou → só guarda o pedido.
            // A bolha será mostrada em HandleSatDown quando o OnSatDown disparar.
        }
    }

    /// <summary>
    /// Chamado pelo CustomerAI quando o cliente senta de verdade.
    /// Mostra a bolha se já tiver pedido sorteado.
    /// </summary>
    void HandleSatDown(CustomerAI ai)
    {
        // só interessa se já existe um pedido
        if (!hasOrder) return;
        if (!bubble || itemAtual == null) return;

        bubble.Show(itemAtual, atrasoDepoisDeSentar);
    }

    public void LimparPedido()
    {
        if (bubble)
            bubble.Hide();

        itemAtual     = null;
        orderedDishId = null;
        hasOrder      = false;
    }

    public bool Matches(Cookable c)
    {
        return c && !string.IsNullOrEmpty(orderedDishId) && c.dishId == orderedDishId;
    }
}
