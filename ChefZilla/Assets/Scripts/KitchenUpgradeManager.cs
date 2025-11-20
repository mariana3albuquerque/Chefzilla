using UnityEngine;

public class KitchenUpgradeManager : MonoBehaviour
{
    public static KitchenUpgradeManager I { get; private set; }

    [Header("Upgrades de tempo de cozimento")]
    [SerializeField, Range(0.25f, 2f)]
    float cookTimeMultiplier = 1f;

    [Header("Upgrades de velocidade do Chef")]
    [SerializeField, Range(0.25f, 3f)]
    float moveSpeedMultiplier = 1f;

    // Propriedades para outros scripts
    public float CookTimeMultiplier  => cookTimeMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        // Se você não quer que esse objeto persista entre cenas, pode comentar a linha abaixo.
        // DontDestroyOnLoad(gameObject);
    }

    // === Chamado quando compra o upgrade de fogão mais rápido ===
    public void SetCookTimeMultiplier(float newMultiplier)
    {
        cookTimeMultiplier = Mathf.Clamp(newMultiplier, 0.25f, 2f);
    }

    // === Chamado quando compra o upgrade de chef mais rápido ===
    public void SetMoveSpeedMultiplier(float newMultiplier)
    {
        moveSpeedMultiplier = Mathf.Clamp(newMultiplier, 0.25f, 3f);
    }

    // === Compatibilidade com o StoveStation (que chama GetCookTimeMultiplier) ===
    public float GetCookTimeMultiplier()
    {
        return cookTimeMultiplier;
    }
}

