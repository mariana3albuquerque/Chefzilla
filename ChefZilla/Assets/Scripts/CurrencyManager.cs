using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager I { get; private set; }

    public int Coins { get; private set; }

    public event Action<int> OnCoinsChanged;

    void Awake()
    {
        if (I && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    // Se no futuro quiser gastar moedas pra upgrade:
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (Coins < amount) return false;

        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    public void ResetCoins()
    {
        Coins = 0;
        OnCoinsChanged?.Invoke(Coins);
    }
}
