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
    public float atrasoDepoisDeSentar = 1.5f;

    public string orderedDishId  { get; private set; }
    public Sprite itemAtual      { get; private set; }

    /// <summary>Prato atualmente pedido pelo cliente.</summary>
    public DishType Requested { get; private set; } = DishType.None;

    /// <summary>Chame assim que o cliente terminar de sentar.</summary>
    public void SolicitarPedido()
    {
        if (!bubble) bubble = GetComponentInChildren<OrderBubble>(true);
        if (!bubble) { Debug.LogWarning("CustomerOrder: sem OrderBubble."); return; }

        if (options == null || options.Length == 0) { Debug.LogWarning("CustomerOrder: sem opções"); return; }

        var opt = options[Random.Range(0, options.Length)];
        orderedDishId = opt.id;
        itemAtual     = opt.icon;

        // Sorteia um dos tipos dispon�veis (ou escolha de outra forma)
        int i = Random.Range(0, icons.Length);
        Requested = icons[i].type;

        bubble.Show(GetIcon(Requested), atrasoDepoisDeSentar);
    }

    /// <summary>Tenta servir um prato. Retorna true se for o correto (e consome o pedido).</summary>
    public bool TryServe(DishType given)
    {
        if (given != DishType.None && given == Requested)
        {
            bubble?.Hide();
            Requested = DishType.None; // consumo do pedido
            return true;
        }
        return false;
    }

    /// <summary>Esconde o bal�o e limpa o pedido atual.</summary>
    public void LimparPedido()
    {
        if (bubble) bubble.Hide();
        itemAtual = null;
        orderedDishId = null;
    }

    public bool Matches(Cookable c)
    {
        return c && !string.IsNullOrEmpty(orderedDishId) && c.dishId == orderedDishId;
    }

    Sprite GetIcon(DishType t)
    {
        if (icons != null)
        {
            for (int i = 0; i < icons.Length; i++)
                if (icons[i].type == t) return icons[i].icon;
        }
        return null;
    }

    // Quando o objeto for desativado/destru�do, esconda sem coroutine
    void OnDisable() { if (bubble) bubble.HideImmediate(); }
    void OnDestroy() { if (bubble) bubble.HideImmediate(); }
}
