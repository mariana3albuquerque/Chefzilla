using UnityEngine;

public class Cookable : MonoBehaviour
{
    [Header("Identificação")]
    public string dishId = "soup";      // usaremos depois para casar com o pedido do NPC
    public string displayName = "Soup"; // só para UI

    [Header("Tempo de preparo")]
    [Min(0.1f)]
    public float cookTime = 3f;         // já é lido pelo StoveStation

    [Header("Pontuação")]
    public int points = 10;             // << defina a pontuação do prato aqui

    // helper para quando formos calcular pontos na entrega
    public int GetPoints() => points;
}
