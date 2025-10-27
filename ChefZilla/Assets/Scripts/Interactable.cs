using UnityEngine;

public enum InteractableType { Stove, Fridge, Table }

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Stove;
    public GameObject spawnPrefab;       // prefab que será instanciado quando o jogador pegar
    public GameObject visualIndicator;   // opcional: um sprite presente na cena para indicar que tem item (não usado na opção A)
}

