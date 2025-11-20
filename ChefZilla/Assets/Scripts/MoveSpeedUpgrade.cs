using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveSpeedUpgrade : MonoBehaviour
{
    [Header("Configuração do upgrade")]
    [SerializeField] int price = 20;                 // quanto custa
    [SerializeField] float multiplierAfterPurchase = 1.4f; // 1.4 = 40% mais rápido

    [Header("UI")]
    [SerializeField] Button button;
    [SerializeField] TMP_Text label;

    bool purchased = false;

    void OnEnable()
    {
        if (CurrencyManager.I != null)
            CurrencyManager.I.OnCoinsChanged += HandleCoinsChanged;

        RefreshUI();
    }

    void OnDisable()
    {
        if (CurrencyManager.I != null)
            CurrencyManager.I.OnCoinsChanged -= HandleCoinsChanged;
    }

    void HandleCoinsChanged(int _)
    {
        RefreshUI();
    }

    // Este é o método que você vai ligar no OnClick do botão
    public void Buy()
    {
        if (purchased) return;
        if (CurrencyManager.I == null) return;

        if (CurrencyManager.I.TrySpend(price))
        {
            purchased = true;

            if (KitchenUpgradeManager.I != null)
                KitchenUpgradeManager.I.SetMoveSpeedMultiplier(multiplierAfterPurchase);

            RefreshUI();
        }
        else
        {
            Debug.Log("[Upgrade] Moedas insuficientes para comprar 'Chef mais rápido'.");
        }
    }

    void RefreshUI()
    {
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;

        if (purchased)
        {
            if (label)
                label.text = "Chef mais rápido\n<color=#00FF88>[Comprado]</color>";
            if (button)
                button.interactable = false;
        }
        else
        {
            if (label)
                label.text = $"Chef mais rápido\n({price} moedas)";
            if (button)
                button.interactable = (coins >= price);
        }
    }
}

