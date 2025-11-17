using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [System.Serializable]
    public struct DishIcon { public DishType type; public Sprite icon; }

    [Header("Balão do Pedido")]
    public OrderBubble bubble;                 // arraste o OrderBubble (pai)
    public float atrasoDepoisDeSentar = 1.5f;

    [Header("Ícones por Prato")]
    public DishIcon[] icons;                   // mapeia tipo -> sprite

    /// <summary>Prato atualmente pedido pelo cliente.</summary>
    public DishType Requested { get; private set; } = DishType.None;

    /// <summary>Chame assim que o cliente terminar de sentar.</summary>
    public void SolicitarPedido()
    {
        if (!bubble) bubble = GetComponentInChildren<OrderBubble>(true);
        if (!bubble) { Debug.LogWarning("CustomerOrder: sem OrderBubble."); return; }

        if (icons == null || icons.Length == 0)
        {
            Requested = DishType.None;
            return;
        }

        // Sorteia um dos tipos disponíveis (ou escolha de outra forma)
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

    /// <summary>Esconde o balão e limpa o pedido atual.</summary>
    public void LimparPedido()
    {
        if (bubble) bubble.Hide();
        Requested = DishType.None;
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

    // Quando o objeto for desativado/destruído, esconda sem coroutine
    void OnDisable() { if (bubble) bubble.HideImmediate(); }
    void OnDestroy() { if (bubble) bubble.HideImmediate(); }
}
