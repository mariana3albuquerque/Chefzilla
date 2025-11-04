using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionZone : MonoBehaviour
{
    [Header("Segurar / Mesas")]
    public Transform holdPoint;
    public float tableSearchRadius = 1.0f;
    public bool allowRAsAlias = false;

    [Header("Cooking")]
    public float defaultCookTime = 2.5f;          // fallback antigo (mantido por compatibilidade)
    public bool blockMovementWhileCooking = true;  // bloqueia só durante o pré-aquecimento

    [Header("Tutorial / Objetivos")]
    [Tooltip("Pode conter Interactables (Fogão, Geladeira) ou TableSpots (mesas)")]
    public GameObject[] objectiveChain;
    public float stopHintDistance = 1.1f;
    public bool turnOffOnlyOnInteract = true;

    int currentObj = -1;
    bool hasActiveHint = false;

    [Header("UI de Interação")]
    public InteractionHintUI interactionHint;   // UIManager com InteractionHintUI

    // estado
    Interactable currentInteractable = null;
    GameObject heldObject = null;

    // refs do Chef
    Animator anim;
    PlayerController2D mover;

    bool isCooking = false;       // usamos somente durante o pré-aquecimento (preheat)
    float cachedMoveSpeed = 0f;

    // --- lógica do “mostra só uma vez” ---
    bool moveHintActive = true;        // começa mostrando dica de movimento
    Vector3 lastPos;                   // para detectar movimento real

    void Awake()
    {
        anim  = GetComponentInParent<Animator>();
        mover = GetComponentInParent<PlayerController2D>();
    }

    void Start()
    {
        AdvanceObjective();

        // mostra a mensagem de movimento uma vez
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
        // 1) se a dica de movimento ainda está ativa, esconda-a ao detectar que o jogador se mexeu
        if (moveHintActive)
        {
            bool pressedArrows =
                Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

            bool actuallyMoved = ((transform.position - lastPos).sqrMagnitude > 0.0001f);

            if (pressedArrows || actuallyMoved)
            {
                moveHintActive = false;
                if (interactionHint != null) interactionHint.HideHint(); // some e não volta mais
            }
            lastPos = transform.position;
        }

        // Durante o pré-aquecimento, só atualizamos a dica de proximidade e saímos
        if (isCooking)
        {
            UpdateProximityHint();
            return;
        }

        // desliga objetivo por distância (se habilitado)
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

        // interação (E / R)
        bool pressedInteract = Input.GetKeyDown(KeyCode.E) || (allowRAsAlias && Input.GetKeyDown(KeyCode.R));
        if (pressedInteract)
        {
            // 1) segurando algo → tentar colocar em mesa livre
            if (heldObject != null)
            {
                TableSpot targetSpot = FindNearestFreeTableSpot();
                if (targetSpot != null)
                {
                    PlaceOnTable(targetSpot);
                    CheckObjectiveProgress(targetSpot.gameObject);
                }
                else
                {
                    Debug.Log("Nenhuma mesa livre próxima.");
                }

                UpdateProximityHint();
                return;
            }

            // 2) Sem item → interagir com o que estiver no trigger
            if (currentInteractable != null)
            {
                // ---- FOGÃO (com StoveStation) ----
                if (currentInteractable.type == InteractableType.Stove)
                {
                    var stove = currentInteractable.GetComponent<StoveStation>();
                    if (stove)
                    {
                        // Se houver prato pronto no fogão, coletar
                        if (stove.HasReadyItem)
                        {
                            var go = stove.CollectReadyItem();
                            if (go != null)
                            {
                                go.transform.SetParent(holdPoint);
                                go.transform.position = holdPoint.position;
                                var rb2 = go.GetComponent<Rigidbody2D>();
                                if (rb2) rb2.simulated = false;
                                heldObject = go;

                                CheckObjectiveProgress(stove.gameObject);
                                UpdateProximityHint();
                            }
                            return;
                        }

                        // Se pode iniciar, roda pré-anim e inicia o cozimento
                        if (stove.CanStart())
                        {
                            StartCoroutine(PreheatThenStart(stove));
                            return;
                        }

                        // Se está ocupado mas sem item pronto, apenas ignore o E (já está cozinhando)
                        return;
                    }

                    // Fallback caso o fogão não tenha StoveStation (mantém comportamento antigo)
                    PickFrom(currentInteractable);
                    CheckObjectiveProgress(currentInteractable.gameObject);
                    UpdateProximityHint();
                    return;
                }

                // ---- GELADEIRA / TABLE (pegar direto) ----
                if (currentInteractable.type == InteractableType.Fridge ||
                    currentInteractable.type == InteractableType.Table)
                {
                    PickFrom(currentInteractable);
                    CheckObjectiveProgress(currentInteractable.gameObject);
                    UpdateProximityHint();
                    return;
                }

                // ---- Interactable genérico (compatibilidade) ----
                PickFrom(currentInteractable);
                CheckObjectiveProgress(currentInteractable.gameObject);
                UpdateProximityHint();
                return;
            }

            // 3) pegar de mesa ocupada (mais próxima)
            TableSpot occupied = FindNearestOccupiedTableSpot();
            if (occupied != null)
            {
                PickFromTable(occupied);
                CheckObjectiveProgress(occupied.gameObject);
            }

            UpdateProximityHint();
        }
        else
        {
            // Quando perto do fogão parado, permitir trocar receita (Z/X) e mostrar dica específica
            if (currentInteractable != null && currentInteractable.type == InteractableType.Stove)
            {
                var stove = currentInteractable.GetComponent<StoveStation>();
                if (stove)
                {
                    if (stove.CanStart())
                    {
                        if (Input.GetKeyDown(KeyCode.Z)) stove.NextRecipe(-1);
                        if (Input.GetKeyDown(KeyCode.X)) stove.NextRecipe(+1);

                        if (!moveHintActive && interactionHint != null)
                            interactionHint.ShowHint($"<b>Z/X</b> troca prato (Atual: <i>{stove.GetActiveRecipeName()}</i>) • <b>E</b> cozinhar");
                        return; // evita sobrescrever a dica aqui embaixo
                    }
                }
            }

            UpdateProximityHint();
        }
    }

    // ===================== COOKING: pré-anim + disparo do fogão =====================
    IEnumerator PreheatThenStart(StoveStation stove)
    {
        if (stove == null) yield break;

        float preheat = Mathf.Max(0f, stove.preheatTime);

        isCooking = true; // bloqueia outras interações apenas durante o pré
        if (blockMovementWhileCooking && mover != null)
        {
            cachedMoveSpeed = mover.moveSpeed;
            mover.moveSpeed = 0f;
        }

        if (anim) anim.SetBool("isCooking", true);

        float t = 0f;
        while (t < preheat)
        {
            // se o jogador sair do fogão, cancela
            if (currentInteractable == null || currentInteractable.GetComponent<StoveStation>() != stove)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        if (anim) anim.SetBool("isCooking", false);
        if (blockMovementWhileCooking && mover != null) mover.moveSpeed = cachedMoveSpeed;

        // inicia o cozimento somente se ainda está no fogão
        if (t >= preheat && currentInteractable != null && currentInteractable.GetComponent<StoveStation>() == stove)
            stove.BeginCooking();

        isCooking = false; // libera o jogador para se mover enquanto o fogão cozinha
        UpdateProximityHint();
    }
    // ==============================================================================

    // ===================== Lógica de pegar/colocar =====================
    void PickFrom(Interactable it)
    {
        if (it == null || it.spawnPrefab == null)
        {
            Debug.LogWarning("Interactable sem spawnPrefab: " + (it ? it.name : "null"));
            return;
        }

        var go = Instantiate(it.spawnPrefab, holdPoint.position, Quaternion.identity);
        go.transform.SetParent(holdPoint);
        go.transform.localPosition = Vector3.zero;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        heldObject = go;

        CheckObjectiveProgress(it.gameObject);
    }

    void PickFromTable(TableSpot spot)
    {
        if (spot == null) return;
        GameObject obj = spot.Remove();
        if (obj == null) return;

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        heldObject = obj;
    }

    TableSpot FindNearestFreeTableSpot()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var spot = c.GetComponent<TableSpot>();
            if (spot == null || spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = spot;
            }
        }

        return best;
    }

    TableSpot FindNearestOccupiedTableSpot()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, tableSearchRadius);
        TableSpot best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var spot = c.GetComponent<TableSpot>();
            if (spot == null || !spot.isOccupied) continue;

            float d = Vector2.SqrMagnitude((Vector2)spot.transform.position - (Vector2)transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = spot;
            }
        }

        return best;
    }

    void PlaceOnTable(TableSpot spot)
    {
        if (heldObject == null || spot == null) return;

        heldObject.transform.SetParent(null);
        spot.Place(heldObject);
        heldObject = null;
    }

    public void DropHeldObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        var rb = heldObject.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;
        heldObject = null;
    }

    // ===================== OBJETIVOS / HINT =====================

    void AdvanceObjective()
    {
        if (currentObj >= 0 && currentObj < objectiveChain.Length && objectiveChain[currentObj] != null)
            SetHintActive(objectiveChain[currentObj], false);

        currentObj++;
        hasActiveHint = false;

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
        if (obj == null) return;

        var i = obj.GetComponent<Interactable>();
        if (i != null) { i.SetHintActive(on); return; }

        var t = obj.GetComponent<TableSpot>();
        if (t != null) { t.SetHintActive(on); return; }
    }

    // Dica de proximidade padrão (quando não estamos no fluxo específico do fogão)
    void UpdateProximityHint()
    {
        if (interactionHint == null) return;
        if (moveHintActive) return; // ainda na dica inicial de movimento

        bool pertoDeAlgo =
            currentInteractable != null ||
            FindNearestFreeTableSpot() != null ||
            FindNearestOccupiedTableSpot() != null;

        if (pertoDeAlgo)
            interactionHint.ShowHint("Aperte <b>E</b> para soltar ou pegar objetos");
        else
            interactionHint.HideHint();
    }

    // Gizmo para ver o raio no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tableSearchRadius);
    }
}
