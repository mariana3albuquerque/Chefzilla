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
        // Captura WASD / setas (Input Manager clássico)
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();

        bool isMoving = input.sqrMagnitude > 0.0001f;
        anim.SetBool("isMoving", isMoving);

        // Guarda última direção pra manter orientação quando parar
        if (isMoving) lastDir = input;

        // ---- FLIP (virar esquerda/direita) ----
        float lookX = isMoving ? input.x : lastDir.x;
        if (Mathf.Abs(lookX) > flipThreshold)
            sr.flipX = (lookX < 0f);

        // ---- TILT (inclinar) ----
        // Regras:
        // - Parado: tilt = 0
        // - Vertical "puro" (|x| pequeno e |y| relevante): tilt = 0
        // - Caso contrário (tem horizontal): tilt segue o sinal de x e a intensidade de |x|
        float targetTilt = 0f;

        if (isMoving)
        {
            bool hasHorizontal = Mathf.Abs(input.x) > horizontalThreshold;
            bool isPureVertical = !hasHorizontal && Mathf.Abs(input.y) > verticalThreshold;

            if (!isPureVertical && hasHorizontal)
            {
                // Inclina para o lado do movimento horizontal
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
            targetTilt = 0f; // parado: sem tilt
        }

        // Aplica a inclinação suavemente (roda o objeto todo; se isso atrapalhar colisão, falo abaixo)
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltSmoothing);
    }

    void FixedUpdate()
    {
        // Movimento com aceleração suave
        Vector2 targetVel = input * moveSpeed;
        Vector2 nextVel = Vector2.MoveTowards(rb.linearVelocity, targetVel, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = nextVel;

        // Alternativa com MovePosition (se preferir):
        // Vector2 desiredPos = rb.position + targetVel * Time.fixedDeltaTime;
        // rb.MovePosition(Vector2.MoveTowards(rb.position, desiredPos, acceleration * Time.fixedDeltaTime));
    }
}
