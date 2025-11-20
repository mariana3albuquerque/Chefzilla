using UnityEngine;

[DisallowMultipleComponent]
public class StoveUpgrade : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] StoveStation stove;
    [SerializeField] Interactable interactable;
    [SerializeField] Collider2D triggerCollider;   // collider de interação (Is Trigger)
    [SerializeField] GameObject lockOverlay;       // filho com o X/cadeado

    [Header("Config do Upgrade")]
    [SerializeField] string displayName = "Fogão extra";
    [SerializeField] int price = 50;
    [SerializeField] bool unlockedAtStart = false;

    public bool IsUnlocked { get; private set; }

    void Reset()
    {
        if (!stove)        stove        = GetComponent<StoveStation>();
        if (!interactable) interactable = GetComponent<Interactable>();

        if (!triggerCollider)
        {
            var col = GetComponent<Collider2D>();
            if (col && col.isTrigger)
                triggerCollider = col;
        }
    }

    void Awake()
    {
        if (!stove)        stove        = GetComponent<StoveStation>();
        if (!interactable) interactable = GetComponent<Interactable>();

        if (!triggerCollider)
        {
            var col = GetComponent<Collider2D>();
            if (col && col.isTrigger)
                triggerCollider = col;
        }
    }

    void Start()
    {
        SetUnlocked(unlockedAtStart);
    }

    public void Unlock()
    {
        if (IsUnlocked) return;
        SetUnlocked(true);
    }

    void SetUnlocked(bool on)
    {
        IsUnlocked = on;

        if (stove)        stove.enabled        = on;
        if (interactable) interactable.enabled = on;
        if (triggerCollider) triggerCollider.enabled = on;

        if (lockOverlay)  lockOverlay.SetActive(!on);
    }

    public string DisplayName => displayName;
    public int Price         => price;
}

