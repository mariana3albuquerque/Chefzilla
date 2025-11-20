using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookSpeedUpgrade : MonoBehaviour
{
    [Header("Configuração do upgrade")]
    [Min(0)]
    public int price = 30;               // quanto custa o upgrade
    [Min(0.01f)]
    public float multiplierAfterPurchase = 0.4f; // depois de comprar: tempo = tempoBase * 0.4

    [Header("UI")]
    [SerializeField] Button button;
    [SerializeField] TMP_Text label;

    bool bought;

    void OnEnable()
    {
        UpdateUI();
    }

    public void TryBuy()
    {
        if (bought) return;
        if (!CurrencyManager.I) return;

        if (CurrencyManager.I.TrySpend(price))
        {
            bought = true;

            if (KitchenUpgradeManager.I)
            {
                KitchenUpgradeManager.I.SetCookTimeMultiplier(multiplierAfterPurchase);
            }

            UpdateUI();
        }
        else
        {
            Debug.Log("[Upgrade] Moedas insuficientes para upgrade de velocidade.");
        }
    }

    void UpdateUI()
    {
        if (!label) return;

        if (bought)
        {
            label.text = "Fogões mais rápidos\n<color=#00FF88>[Comprado]</color>";
            if (button) button.interactable = false;
        }
        else
        {
            label.text = $"Fogões mais rápidos\n({price} moedas)";
            if (button)
            {
                bool canAfford = CurrencyManager.I && CurrencyManager.I.Coins >= price;
                button.interactable = canAfford;
            }
        }
    }
}

