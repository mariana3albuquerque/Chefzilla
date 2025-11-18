using UnityEngine;

public enum StoveState { Idle, Prepping, Cooking, Ready }

[RequireComponent(typeof(Collider2D))]
public class StoveStation : MonoBehaviour
{
    [Header("Refs")]
    public Interactable interactable;           // opcional; se null, pega no Awake
    public Transform spawnPoint;                // onde o prato pronto aparece/aguarda
    public GameObject progressBarPrefab;        // prefab com ProgressBarUI (tem Set01 e AttachTo)

    [Header("Timings (s)")]
    public float preheatTime = 0.6f;            // pré-animação do Chef antes da barra
    public float defaultCookTime = 3f;          // fallback se o prefab não tiver Cookable

    [Header("Receitas (opcional)")]
    [Tooltip("Se preencher, o fogão usa essa lista para escolher a receita atual (Z/X para trocar).")]
    public GameObject[] recipeOptions;
    [SerializeField] int recipeIndex = 0;       // receita selecionada

    [Header("Som de comida pronta")]
    [SerializeField] AudioClip foodReadySFX;            // som quando o prato fica pronto
    [SerializeField, Range(0f, 1f)] float foodReadyVolume = 1f;

    public StoveState State { get; private set; } = StoveState.Idle;

    // ==== Consulta simples para UI / Player ====
    public bool HasReadyItem => State == StoveState.Ready;
    public bool CanStart() => State == StoveState.Idle;
    public bool IsBusy() => State == StoveState.Prepping || State == StoveState.Cooking || State == StoveState.Ready;

    public string GetActiveRecipeName()
    {
        var p = GetActiveRecipe();
        return p ? p.name : "(sem receita)";
    }

    public void NextRecipe(int dir = 1)
    {
        if (recipeOptions == null || recipeOptions.Length == 0) return;
        recipeIndex = (recipeIndex + dir + recipeOptions.Length) % recipeOptions.Length;
    }

    // ============ Fluxo de cozimento ============
    public void BeginCooking()
    {
        if (!CanStart()) return;
        StopAllCoroutines();
        StartCoroutine(CookRoutine());
    }

    // ---------- Internos ----------
    ProgressBarUI progressUI;
    GameObject readyInstance;

    GameObject GetActiveRecipe()
    {
        // 1) Se houver lista de receitas, usa a selecionada
        if (recipeOptions != null && recipeOptions.Length > 0)
            return recipeOptions[Mathf.Clamp(recipeIndex, 0, recipeOptions.Length - 1)];

        // 2) Caso contrário, usa o Spawn Prefab do Interactable (comportamento antigo)
        return interactable ? interactable.spawnPrefab : null;
    }

    float GetCookTime()
    {
        float t = defaultCookTime;
        var prefab = GetActiveRecipe();
        if (prefab)
        {
            var c = prefab.GetComponent<Cookable>();
            if (c && c.cookTime > 0f) t = c.cookTime;
        }
        return Mathf.Max(0.01f, t);
    }

    System.Collections.IEnumerator CookRoutine()
    {
        State = StoveState.Cooking;
        EnsureProgressUI(true);

        float duration = GetCookTime();
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            if (progressUI) progressUI.Set01(t / duration);
            yield return null;
        }

        EnsureProgressUI(false);
        SpawnReadyItem();
        State = StoveState.Ready;
    }

    void SpawnReadyItem()
    {
        var prefab = GetActiveRecipe();
        if (!prefab) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        readyInstance = Instantiate(prefab, pos, Quaternion.identity);

        if (spawnPoint) readyInstance.transform.SetParent(spawnPoint); // fica “sobre” o fogão
        var rb = readyInstance.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        // 🔊 som de comida pronta
        PlayFoodReadySFX();
    }

    void PlayFoodReadySFX()
    {
        if (foodReadySFX == null) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        AudioSource.PlayClipAtPoint(foodReadySFX, pos, foodReadyVolume);
    }

    public GameObject CollectReadyItem()
    {
        return TryTakeReady(out var item) ? item : null;
    }

    public bool TryTakeReady(out GameObject item)
    {
        if (State != StoveState.Ready || !readyInstance)
        {
            item = null;
            return false;
        }
        item = readyInstance;
        readyInstance = null;
        State = StoveState.Idle;
        return true;
    }

    void EnsureProgressUI(bool show)
    {
        if (show)
        {
            if (!progressUI && progressBarPrefab)
            {
                var go = Instantiate(progressBarPrefab, (spawnPoint ? spawnPoint.position : transform.position), Quaternion.identity);
                progressUI = go.GetComponent<ProgressBarUI>();
                if (progressUI) progressUI.AttachTo(spawnPoint ? spawnPoint : transform);
                progressUI?.Set01(0f);
            }
            if (progressUI) progressUI.gameObject.SetActive(true);
        }
        else
        {
            if (progressUI) progressUI.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        if (!interactable) TryGetComponent(out interactable);
        if (TryGetComponent<Collider2D>(out var col)) col.isTrigger = true;
    }

    void OnDisable()
    {
        if (progressUI) progressUI.gameObject.SetActive(false);
    }
}
