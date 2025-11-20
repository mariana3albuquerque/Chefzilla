using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeStoveUI : MonoBehaviour   // <- TEM QUE SER ESSE NOME
{
    [Header("Moedas")]
    [SerializeField] TMP_Text coinsText;

    [Header("Fogão esquerdo")]
    [SerializeField] StoveUpgrade leftStove;
    [SerializeField] Button leftButton;
    [SerializeField] TMP_Text leftLabel;

    [Header("Fogão direito")]
    [SerializeField] StoveUpgrade rightStove;
    [SerializeField] Button rightButton;
    [SerializeField] TMP_Text rightLabel;

    void OnEnable()
    {
        if (CurrencyManager.I)
            CurrencyManager.I.OnCoinsChanged += HandleCoinsChanged;

        Refresh();
    }

    void OnDisable()
    {
        if (CurrencyManager.I)
            CurrencyManager.I.OnCoinsChanged -= HandleCoinsChanged;
    }

    void HandleCoinsChanged(int _)
    {
        Refresh();
    }

    public void BuyLeft()  => TryBuy(leftStove);
    public void BuyRight() => TryBuy(rightStove);

    void TryBuy(StoveUpgrade stove)
    {
        if (!stove || stove.IsUnlocked) return;
        if (!CurrencyManager.I) return;

        if (CurrencyManager.I.TrySpend(stove.Price))
        {
            stove.Unlock();
            Refresh();
        }
        else
        {
            Debug.Log("[Upgrade] Moedas insuficientes para comprar " + stove.DisplayName);
        }
    }

    void Refresh()
    {
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;
        if (coinsText)
            coinsText.text = $"Moedas: {coins}";

        RefreshStove(leftStove, leftButton, leftLabel);
        RefreshStove(rightStove, rightButton, rightLabel);
    }

    void RefreshStove(StoveUpgrade stove, Button btn, TMP_Text label)
    {
        if (!stove)
        {
            if (btn)   btn.interactable = false;
            if (label) label.text = "-";
            return;
        }

        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;

        if (stove.IsUnlocked)
        {
            if (label)
                label.text = $"{stove.DisplayName}\n<color=#00FF88>[Comprado]</color>";
            if (btn) btn.interactable = false;
        }
        else
        {
            if (label)
                label.text = $"{stove.DisplayName}\nComprar ({stove.Price} moedas)";
            if (btn) btn.interactable = (coins >= stove.Price);
        }
    }
}

