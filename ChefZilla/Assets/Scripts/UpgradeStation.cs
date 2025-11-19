using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    [SerializeField] UpgradeMenu upgradeMenu;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (upgradeMenu == null) return;

        // como o Chef tem colisor em filhos, pegamos o PlayerController2D no parent
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player != null)
        {
            upgradeMenu.Open();
        }
    }
}

