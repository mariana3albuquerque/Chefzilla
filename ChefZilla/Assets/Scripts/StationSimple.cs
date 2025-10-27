using UnityEngine;

public class StationSimple : MonoBehaviour
{
    [Tooltip("Prefab do item que começa na estação (ex: Prato).")]
    public GameObject initialItemPrefab;

    [Tooltip("Transform onde o item ficará ao aparecer na estação.")]
    public Transform itemSpawnPoint;

    private GameObject currentItem;

    void Start()
    {
        if (itemSpawnPoint == null)
        {
            GameObject sp = new GameObject("ItemSpawnPoint");
            sp.transform.parent = transform;
            sp.transform.localPosition = Vector3.zero;
            itemSpawnPoint = sp.transform;
        }

        if (initialItemPrefab != null)
        {
            SpawnInitialItem();
        }
    }

    void SpawnInitialItem()
    {
        GameObject obj = Instantiate(initialItemPrefab, itemSpawnPoint.position, Quaternion.identity);
        // marca como item e desliga colisão (está na estação)
        obj.tag = "Item";
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        Rigidbody2D rbItem = obj.GetComponent<Rigidbody2D>();
        if (rbItem != null) Destroy(rbItem);
        obj.transform.SetParent(itemSpawnPoint);
        obj.transform.localPosition = Vector3.zero;
        currentItem = obj;
    }

    public void InteractWithPlayer(PlayerSimple player)
    {
        if (player == null) return;

        GameObject held = player.GetHeldItem();

        // se o player não está segurando e a estação tem item → player pega
        if (held == null && currentItem != null)
        {
            currentItem.transform.SetParent(null);
            Collider2D col = currentItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
            player.Pickup(currentItem);
            currentItem = null;
            return;
        }

        // se o player está segurando e a estação está vazia → player coloca
        if (held != null && currentItem == null)
        {
            GameObject item = player.RemoveHeldItem();
            item.transform.SetParent(itemSpawnPoint);
            item.transform.localPosition = Vector3.zero;
            Collider2D col = item.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            Rigidbody2D rbItem = item.GetComponent<Rigidbody2D>();
            if (rbItem != null) Destroy(rbItem);
            currentItem = item;
            return;
        }

        // caso contrário: nada a fazer
    }

    void OnDrawGizmosSelected()
    {
        if (itemSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemSpawnPoint.position, 0.12f);
        }
    }
}

