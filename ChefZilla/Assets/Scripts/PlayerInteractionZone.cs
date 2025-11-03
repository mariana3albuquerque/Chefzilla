using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionZone : MonoBehaviour
{
    // ====== Segurar / Mesas ======
    [Header("Segurar / Mesas")]
    public Transform holdPoint;               // opcional; se null, usa offset local no Chef
    public Vector2 holdLocalOffset = new Vector2(0f, 0.35f);
    public float tableSearchRadius = 1.0f;
    public bool allowRAsAlias = false;        // (opcional) R como alias de E

    // ====== UI / Tutorial / Objetivos ======
    [Header("UI de Interação (opcional)")]
    public InteractionHintUI interactionHint; // seu componente de UI para mensagens

    [Header("Tutorial / Objetivos")]
    [Tooltip("Coloque aqui os GameObjects que o jogador deve visitar em ordem (Fogões, Mesas, Geladeira, etc.).")]
    public GameObject[] objectiveChain;
    public float stopHintDistance = 1.1f;     // distância para 'atingir' um objetivo, se não exigir apertar E
    public bool turnOffOnlyOnInteract = true; // se true: só avança objetivo quando interagir (E). Se false: chegar perto já conta.

    // estado geral
    Interactable currentInteractable = null;
    GameObject heldObject = null;

    // refs do Chef
    Animator anim;
    PlayerController2D mover;

    // pré-animação do Chef (antes de começar a cozinhar)
    bool playingPreheat = false;
    float cachedMoveSpeed = 0f;

    // dica de movimento "uma vez só"
    bool moveHintActive = true;
    Vector3 lastPos;

    // objetivos
    int  currentObj   = -1;
    bool hasActiveHint = false;

    void Awake()
    {
        anim  = GetComponentInParent<Animator>();
        mover = GetComponentInParent<PlayerController2D>();
    }

    void Start()
    {
        AdvanceObjective();

        // Mostra apenas uma vez a dica de movimento
        if (interactionHint != null)
            interactionHint.ShowHint("Use as setas do teclado para mover o chef");

        lastPos = transform.position;
    }

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it != null) currentInteractable = it;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it != null && it == currentInteractable) currentInteractable = null;
    }

    void Update()
    {
        // --- esconder a dica de movimento quando o jogador realmente se mexer/pressionar setas
        if (moveHintActive)
        {
            bool pressedArrows =
                Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

            bool actuallyMoved = ((transform.position - lastPos).sqrMagnitude > 0.0001f);

            if (pressedArrows || actuallyMoved)
            {
                moveHintActive = false;
                if (interactionHint != null) interactionHint.HideHint();
            }
            lastPos = transform.position;
        }

        if (playingPreheat)
        {
            UpdateProximityHint();
            return;
        }

        // Se a cadeia de objetivos avança só por proximidade (sem apertar E)
        if (hasActiveHint && !turnOffOnlyOnInteract)
        {
            GameObject target = GetCurrentObjective();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist <= stopHintDistance)
                {
                    SetHintActive(target, false);
                    hasActiveHint = false;
                    AdvanceObjective();
                }
            }
        }

        // --- Interação (E / R) ---
        if (Input.GetKeyDown(KeyCode.E) || (allowRAsAlias && Input.GetKeyDown(KeyCode.R)))
        {
            // 1) segurando algo -> tenta colocar em mesa livre
            if (heldObject != null)
            {
                var free = FindNearestFreeTableSpot();
                if (free != null)
                {
                    PlaceOnTable(free);
                    CheckObjectiveProgress(free.gameObject);
                }
                else
                {
                    Debug.Log("Nenhuma mesa livre próxima.");
                }

                UpdateProximityHint();
                return;
            }

            // 2) não segurando -> interagir com o que está no trigger
            if (currentInteractable != null)
            {
                // ----- FOGÃO -----
                if (currentInteractable.type == InteractableType.Stove)
                {
                    var stove = currentInteractable.GetComponent<StoveStation>();
                    if (!stove) { Debug.LogWarning("Stove sem StoveStation."); return; }

                    // Se já tem prato pronto, coletar
                    if (stove.HasReadyItem)
                    {
                        var item = stove.CollectReadyItem();
                        if (item) Hold(item);
                        CheckObjectiveProgress(stove.gameObject);
                        UpdateProximityHint();
                        return;
                    }

                    // Se está livre, iniciar (pré-anim + cozimento)
                    if (stove.CanStart())
                    {
                        StartCoroutine(PreheatThenStart(stove));
                        // A validação do objetivo por "interagir no fogão" acontece ao final da pré-anim:
                        // (veja o CheckObjectiveProgress no fim da coroutine)
                        return;
                    }

                    // Ocupado (preparando/cozinhando): nada a fazer
                    Debug.Log("Fogão ocupado.");
                    UpdateProximityHint();
                    return;
                }

                // ----- OUTROS INTERACTABLES -----
                if (currentInteractable.type == InteractableType.Fridge ||
                    currentInteractable.type == InteractableType.Table)
                {
                    PickFrom(currentInteractable);
                    CheckObjectiveProgress(currentInteractable.gameObject);
                    UpdateProximityHint();
                    return;
                }

                // genérico
                PickFrom(currentInteractable);
                CheckObjectiveProgress(currentInteractable.gameObject);
                UpdateProximityHint();
                return;
            }

            // 3) sem interactable por perto -> pegar de mesa ocupada
            var occ = FindNearestOccupiedTableSpot();
            if (occ != null)
            {
                PickFromTable(occ);
                CheckObjectiveProgress(occ.gameObject);
                UpdateProximityHint();
                return;
            }

            Debug.Log("Nada para pegar por perto.");
            UpdateProximityHint();
        }
        else
        {
            UpdateProximityHint();
        }
    }

    // ===================== COROUTINE: PRÉ-ANIM + START STOVE =====================
    IEnumerator PreheatThenStart(StoveStation stove)
    {
        playingPreheat = true;

        if (mover)
        {
            cachedMoveSpeed = mover.moveSpeed;
            mover.moveSpeed = 0f;
        }
        if (anim) anim.SetBool("isCooking", true);

        float t = 0f, preheat = stove.GetPreheatTime();
        var expected = currentInteractable; // fogão alvo

        while (t < preheat)
        {
            if (currentInteractable != expected) break; // se o jogador sair do fogão, cancela
            t += Time.deltaTime;
            yield return null;
        }

        if (anim) anim.SetBool("isCooking", false);
        if (mover) mover.moveSpeed = cachedMoveSpeed;
        playingPreheat = false;

        if (t >= preheat && currentInteractable == expected)
        {
            stove.BeginCooking();                        // inicia cozimento (barra aparece)
            CheckObjectiveProgress(stove.gameObject);    // conta "interagiu com o fogão"
        }

        UpdateProximityHint();
    }
    // ============================================================================

    // ===================== Pegar / Segurar / Soltar =====================
    void PickFrom(Interactable it)
    {
        if (!it || !it.spawnPrefab) { Debug.LogWarning("Interactable sem spawnPrefab."); return; }

        var obj = Instantiate(it.spawnPrefab, transform.position, Quaternion.identity);
        Hold(obj);
    }

    void Hold(GameObject obj)
    {
        if (!obj) return;

        Transform parent = holdPoint ? holdPoint : transform.parent; // InteractionZone é filho do Chef
        obj.transform.SetParent(parent);
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = holdPoint ? Vector3.zero : (Vector3)holdLocalOffset;

        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        heldObject = obj;
    }

    void PickFromTable(TableSpot spot)
    {
        if (!spot) return;
        var obj = spot.Remove();
        if (!obj) return;
        Hold(obj);
    }

    void PlaceOnTable(TableSpot spot)
    {
        if (!heldObject || !spot) return;
        heldObject.transform.SetParent(null);
        spot.Place(heldObject);
        heldObject = null;
    }

    public void DropHeldObject()
    {
        if (!heldObject) return;
        heldObject.transform.SetParent(null);
        var rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;
        heldObject = null;
    }

    // ===================== Busca de mesas =====================
    TableSpot FindNearestFreeTableSpot()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null; float bestD = float.MaxValue;
        foreach (var c in cols)
        {
            var s = c.GetComponent<TableSpot>();
            if (!s || s.isOccupied) continue;
            float d = ((Vector2)s.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }

    TableSpot FindNearestOccupiedTableSpot()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null; float bestD = float.MaxValue;
        foreach (var c in cols)
        {
            var s = c.GetComponent<TableSpot>();
            if (!s || !s.isOccupied) continue;
            float d = ((Vector2)s.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }

    // ===================== Objetivos / Hints =====================
    void AdvanceObjective()
    {
        // desliga o anterior
        if (currentObj >= 0 && currentObj < objectiveChain.Length && objectiveChain[currentObj] != null)
            SetHintActive(objectiveChain[currentObj], false);

        currentObj++;
        hasActiveHint = false;

        // liga o próximo
        if (currentObj < objectiveChain.Length && objectiveChain[currentObj] != null)
        {
            SetHintActive(objectiveChain[currentObj], true);
            hasActiveHint = true;
        }
    }

    void CheckObjectiveProgress(GameObject obj)
    {
        if (currentObj >= 0 && currentObj < objectiveChain.Length && objectiveChain[currentObj] == obj)
        {
            SetHintActive(obj, false);
            hasActiveHint = false;
            AdvanceObjective();
        }
    }

    GameObject GetCurrentObjective()
    {
        if (currentObj >= 0 && currentObj < objectiveChain.Length)
            return objectiveChain[currentObj];
        return null;
    }

   void SetHintActive(GameObject obj, bool on)
{
    if (!obj) return;

    // Tenta achar Interactable no próprio GO, nos filhos e nos pais
    Interactable it =
        obj.GetComponent<Interactable>() ??
        obj.GetComponentInChildren<Interactable>(true) ??
        obj.GetComponentInParent<Interactable>();

    if (it != null)
    {
        it.SetHintActive(on);
        return;
    }

    // Tenta TableSpot (caso você use mesa como objetivo)
    TableSpot spot =
        obj.GetComponent<TableSpot>() ??
        obj.GetComponentInChildren<TableSpot>(true) ??
        obj.GetComponentInParent<TableSpot>();

    if (spot != null)
    {
        // Se seu TableSpot tiver SetHintActive, descomente:
        // spot.SetHintActive(on);

        // Fallback: procura um filho chamado "Indicador" e liga/desliga
        Transform ind = spot.transform.Find("Indicador");
        if (ind != null) ind.gameObject.SetActive(on);
        return;
    }

    // Último fallback: tenta um filho chamado "Indicador" no próprio objeto
    Transform indicator = obj.transform.Find("Indicador");
    if (indicator != null) indicator.gameObject.SetActive(on);
}

    void UpdateProximityHint()
    {
        if (interactionHint == null) return;
        if (moveHintActive) return; // enquanto a dica de movimento está ativa, não trocamos

        bool pertoDeAlgo =
            currentInteractable != null ||
            FindNearestFreeTableSpot() != null ||
            FindNearestOccupiedTableSpot() != null;

        if (pertoDeAlgo) interactionHint.ShowHint("Aperte <b>E</b> para interagir");
        else             interactionHint.HideHint();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tableSearchRadius);
    }
}
