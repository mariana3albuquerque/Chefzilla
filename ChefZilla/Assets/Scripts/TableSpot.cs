using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TableSpot : MonoBehaviour
{
    [Header("Estado da Mesa")]
    public bool isOccupied = false;          // se já tem item
    public GameObject placedObject = null;   // referência ao item na mesa

    [Header("Indicador Visual (Hint Circle)")]
    public GameObject visualIndicator;       

    // Colocar um item na mesa
    public void Place(GameObject obj)
    {
        if (isOccupied) return;

        // parent e snap
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // desativa física
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        placedObject = obj;
        isOccupied = true;
    }

    // Remover o item e devolver pro jogador
    public GameObject Remove()
    {
        if (!isOccupied || placedObject == null) return null;

        GameObject obj = placedObject;
        placedObject = null;
        isOccupied = false;

        // reativa física
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        // desapega do parent
        obj.transform.SetParent(null);
        return obj;
    }

    // limpar manualmente
    public void Clear()
    {
        placedObject = null;
        isOccupied = false;
    }

    // Ativa/desativa o círculo de dica
    public void SetHintActive(bool on)
    {
        if (visualIndicator != null)
            visualIndicator.SetActive(on);
    }
}

