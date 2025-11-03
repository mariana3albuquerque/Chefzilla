using UnityEngine;

public enum InteractableType { Stove, Fridge, Table }

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Stove;
    public GameObject spawnPrefab;     // prefab que será instanciado quando o jogador pegar
    public GameObject visualIndicator; // opcional

    [Header("Cooking (para Stove)")]
    public float cookingTime = 2.5f;   // tempo que vai ficar cozinhando

    public void SetHintActive(bool on)
    {
        if (visualIndicator != null)
            visualIndicator.SetActive(on);
    }
}
