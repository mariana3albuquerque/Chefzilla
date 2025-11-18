using UnityEngine;

public class Cookable : MonoBehaviour
{
    [Header("Identificação")]
    public string dishId = "soup";
    public string displayName = "Soup";

    [Header("Tempo de preparo")]
    [Min(0.1f)]
    public float cookTime = 3f;

    [Header("Tempo de consumo (s)")]
    [Min(0.5f)]
    public float eatTime = 5f;

    [Header("Pontuação")]
    public int points = 10;

    [Header("Recompensa em moedas")]
    public int coinsReward = 5;

    public int GetPoints() => points;
    public int GetCoinsReward() => coinsReward;
}
