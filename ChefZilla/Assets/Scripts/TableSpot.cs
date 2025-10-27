using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TableSpot : MonoBehaviour
{
    public bool isOccupied = false;
    // referência ao objeto que está na mesa (se houver)
    public GameObject placedObject = null;

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

    // Remove e retorna o objeto que estava na mesa (ou null)
    public GameObject Remove()
    {
        if (!isOccupied || placedObject == null) return null;

        GameObject obj = placedObject;
        placedObject = null;
        isOccupied = false;

        // reativa física (opcional — caso queira que o item tenha física após soltar)
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        // importantíssimo: desapega do parent (o código chamador pode reparentar novamente)
        obj.transform.SetParent(null);
        return obj;
    }

    // opcional: limpar manualmente
    public void Clear()
    {
        placedObject = null;
        isOccupied = false;
    }
}

