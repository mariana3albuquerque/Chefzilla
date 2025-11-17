using UnityEngine;

public class Cookable : MonoBehaviour
{
    [Header("Identificação")]
    public string dishId = "soup";      // deve bater com o id pedido pelo cliente
    public string displayName = "Soup";

    [Header("Tempo de preparo")]
    [Min(0.1f)]
    public float cookTime = 3f;

    [Header("Tempo de consumo (s)")]
    [Min(0.5f)]
    public float eatTime = 5f;          // quanto tempo o cliente leva para comer

    [Header("Pontuação")]
    public int points = 10;

    public int GetPoints() => points;
}
