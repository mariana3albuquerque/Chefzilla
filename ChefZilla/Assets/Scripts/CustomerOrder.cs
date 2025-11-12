using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    public OrderBubble bubble;
    public Sprite[] itensPossiveis;          // ícones dos itens (pizza, sopa…)
    public float atrasoDepoisDeSentar = 1.5f;

    public Sprite itemAtual { get; private set; }

    public void SolicitarPedido()
    {
        if (!bubble) bubble = GetComponentInChildren<OrderBubble>(true);
        if (!bubble) { Debug.LogWarning("CustomerOrder: sem OrderBubble"); return; }

        itemAtual = (itensPossiveis != null && itensPossiveis.Length > 0)
            ? itensPossiveis[Random.Range(0, itensPossiveis.Length)]
            : null;

        bubble.Show(itemAtual, atrasoDepoisDeSentar);
    }

    public void LimparPedido()
    {
        if (bubble) bubble.Hide();
        itemAtual = null;
    }
}
