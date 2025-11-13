using UnityEngine;
using System; // << NOVO (pros eventos)

[RequireComponent(typeof(Collider2D))]
public class TableSpot : MonoBehaviour
{
    [Header("Estado da Mesa")]
    public bool isOccupied = false;
    public GameObject placedObject = null;

    [Header("Indicador Visual (Hint Circle)")]
    public GameObject visualIndicator;

    // << NOVO: eventos para notificar quem estiver observando a mesa
    public event Action<TableSpot, GameObject> OnPlaced;
    public event Action<TableSpot> OnCleared;

    // helper opcional
    public Cookable GetCookable() => placedObject ? placedObject.GetComponent<Cookable>() : null;

    // Colocar um item na mesa
    public void Place(GameObject obj)
    {
        if (isOccupied) return;

        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        placedObject = obj;
        isOccupied = true;

        // << NOVO
        OnPlaced?.Invoke(this, obj);
    }

    // Remover o item e devolver pro jogador
    public GameObject Remove()
    {
        if (!isOccupied || placedObject == null) return null;

        GameObject obj = placedObject;
        placedObject = null;
        isOccupied = false;

        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        obj.transform.SetParent(null);

        // << NOVO
        OnCleared?.Invoke(this);
        return obj;
    }

    public void Clear()
    {
        placedObject = null;
        isOccupied = false;

        // << NOVO
        OnCleared?.Invoke(this);
    }

    public void SetHintActive(bool on)
    {
        if (visualIndicator != null)
            visualIndicator.SetActive(on);
    }
}
