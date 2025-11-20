using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.8f;      // velocidade alvo
    public float acceleration = 18f;    // arranque/freada suave

    [Header("Facing/Flip")]
    public float flipThreshold = 0.01f; // evita flip "louco" quando quase parado

    [Header("Tilt (inclinação)")]
    public float tiltAngle = 3f;        // graus de inclinação máxima
    public float tiltSmoothing = 0.15f; // 0.1–0.2 fica suave
    public float horizontalThreshold = 0.05f; // quanto de X precisa pra considerar "horizontal"
    public float verticalThreshold   = 0.05f; // quanto de Y precisa pra considerar "vertical"

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    Vector2 input;
    Vector2 lastDir = Vector2.right;    // para manter o "lado" quando para

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // --- NOVO: se estiver cozinhando, bloqueia input e movimento/anim de locomoção ---
        bool cooking = anim.GetBool("isCooking");

        if (cooking)
            input = Vector2.zero;
        else
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 1f) input.Normalize();

        bool isMoving = !cooking && input.sqrMagnitude > 0.0001f;
        anim.SetBool("isMoving", isMoving);

        // Guarda última direção pra manter orientação quando parar (e durante o cooking)
        if (isMoving) lastDir = input;

        // ---- FLIP (virar esquerda/direita) ----
        float lookX = isMoving ? input.x : lastDir.x;
        if (Mathf.Abs(lookX) > flipThreshold)
            sr.flipX = (lookX < 0f);

        // ---- TILT (inclinar) ----
        // Parado e cozinhando: tilt = 0
        float targetTilt = 0f;

        if (isMoving)
        {
            bool hasHorizontal = Mathf.Abs(input.x) > horizontalThreshold;
            bool isPureVertical = !hasHorizontal && Mathf.Abs(input.y) > verticalThreshold;

            if (!isPureVertical && hasHorizontal)
            {
                float strength = Mathf.Clamp01(Mathf.Abs(input.x)); // 0..1
                targetTilt = tiltAngle * Mathf.Sign(input.x) * strength;
            }
            else
            {
                targetTilt = 0f; // subindo/descendo: sem tilt
            }
        }
        else
        {
            targetTilt = 0f; // parado ou cozinhando: sem tilt
        }

        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltSmoothing);
    }

    void FixedUpdate()
{
    // Se estiver cozinhando, não anda
    Vector2 effectiveInput = anim.GetBool("isCooking") ? Vector2.zero : input;

    // 🔥 pega o multiplicador de velocidade do upgrade
    float speedMult = 1f;
    if (KitchenUpgradeManager.I != null)
        speedMult = KitchenUpgradeManager.I.MoveSpeedMultiplier;

    // Aplica velocidade base * multiplicador
    Vector2 targetVel = effectiveInput * (moveSpeed * speedMult);

    Vector2 nextVel = Vector2.MoveTowards(
        rb.linearVelocity,
        targetVel,
        acceleration * Time.fixedDeltaTime
    );

    rb.linearVelocity = nextVel;
}

}
