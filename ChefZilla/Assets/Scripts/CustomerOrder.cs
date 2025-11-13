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

    public void SolicitarPedido()
    {
        if (!bubble) bubble = GetComponentInChildren<OrderBubble>(true);
        if (!bubble) { Debug.LogWarning("CustomerOrder: sem OrderBubble"); return; }

        if (options == null || options.Length == 0) { Debug.LogWarning("CustomerOrder: sem opções"); return; }

        var opt = options[Random.Range(0, options.Length)];
        orderedDishId = opt.id;
        itemAtual     = opt.icon;

        bubble.Show(itemAtual, atrasoDepoisDeSentar);
    }

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
}
