using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HintPulse : MonoBehaviour
{
    [Header("Pulse")]
    public float speed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 0.9f;
    public bool rotateSlightly = false;
    public float rotateSpeed = 25f;

    SpriteRenderer sr;
    Vector3 baseScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0..1
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * s;

        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            sr.color = c;
        }

        if (rotateSlightly)
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}

