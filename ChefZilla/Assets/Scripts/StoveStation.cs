using UnityEngine;

public enum StoveState { Idle, Prepping, Cooking, Ready }

[RequireComponent(typeof(Collider2D))]
public class StoveStation : MonoBehaviour
{
    [Header("Refs")]
    public Interactable interactable;           // se null, pega no Awake
    public Transform spawnPoint;                // onde o prato fica pronto/aguardando
    public GameObject progressBarPrefab;        // prefab com ProgressBarUI (Canvas World Space)

    [Header("Timings (s)")]
    public float preheatTime = 0.6f;            // duração da pré-anim do Chef
    public float defaultCookTime = 3f;          // fallback

    public StoveState State { get; private set; } = StoveState.Idle;

    public bool HasReadyItem => State == StoveState.Ready;
    public bool CanStart() => State == StoveState.Idle;
    public bool IsBusy()   => State == StoveState.Prepping || State == StoveState.Cooking || State == StoveState.Ready;

    public float GetPreheatTime() => Mathf.Max(0f, preheatTime);

    public float GetCookTime()
    {
        // prioridade: Cookable do prato > Interactable.cookingTime > defaultCookTime
        float t = defaultCookTime;

        if (interactable)
        {
            if (interactable.cookingTime > 0f) t = interactable.cookingTime;

            if (interactable.spawnPrefab)
            {
                var cook = interactable.spawnPrefab.GetComponent<Cookable>();
                if (cook && cook.cookTime > 0f) t = cook.cookTime;
            }
        }
        return Mathf.Max(0.01f, t);
    }

    // aliases caso chame por nomes diferentes
    public void BeginCook() => BeginCooking();
    public GameObject CollectReadyItem() => TryTakeReady(out var item) ? item : null;

    ProgressBarUI progressUI;
    GameObject readyInstance;

    public void BeginCooking()
    {
        if (!CanStart()) return;
        StopAllCoroutines();
        StartCoroutine(CookRoutine());
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
        if (!interactable || !interactable.spawnPrefab) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        readyInstance = Instantiate(interactable.spawnPrefab, pos, Quaternion.identity);
        if (spawnPoint) readyInstance.transform.SetParent(spawnPoint);

        var rb = readyInstance.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
    }

    public bool TryTakeReady(out GameObject item)
    {
        if (State != StoveState.Ready || !readyInstance)
        {
            item = null; return false;
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

                // garantir World Space corretamente configurado
                var canvas = go.GetComponent<Canvas>();
                if (canvas)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 500;
                }

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
