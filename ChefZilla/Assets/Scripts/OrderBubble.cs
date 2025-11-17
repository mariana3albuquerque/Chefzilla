using UnityEngine;
using System.Collections;

public class OrderBubble : MonoBehaviour
{
    [Header("Renderers")]
    public SpriteRenderer bg;     // SpriteRenderer do balão (fundo)
    public SpriteRenderer icon;   // SpriteRenderer do ícone (filho)

    [Header("Aparência/Posição")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);
    public float popDuration = 0.18f;

    Coroutine co;

    void Awake()
    {
        // Começa invisível
        transform.localScale = Vector3.zero;
        if (bg) bg.enabled = false;
        if (icon) icon.enabled = false;
    }

    /// <summary>Mostra o balão com o ícone informado (opcionalmente com atraso).</summary>
    public void Show(Sprite itemIcon, float delay = 0f)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowCo(itemIcon, delay));
    }

    IEnumerator ShowCo(Sprite itemIcon, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (icon) icon.sprite = itemIcon;
        if (bg) bg.enabled = true;
        if (icon) icon.enabled = true;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popDuration);
            // de 0 → 1.1, depois fixamos em 1
            transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.1f, k);
            yield return null;
        }
        transform.localScale = Vector3.one;
        co = null;
    }

    /// <summary>Esconde com animação (se possível). Se o GO estiver inativo, esconde instantaneamente.</summary>
    public void Hide()
    {
        if (co != null) StopCoroutine(co);

        // Se não der para rodar coroutine (GO desativado), faça instantâneo
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            HideImmediate();
            return;
        }

        co = StartCoroutine(HideCo());
    }

    /// <summary>Esconde imediatamente, sem animação.</summary>
    public void HideImmediate()
    {
        if (co != null) StopCoroutine(co);
        if (bg) bg.enabled = false;
        if (icon) icon.enabled = false;
        transform.localScale = Vector3.zero;
        co = null;
    }

    IEnumerator HideCo()
    {
        float dur = popDuration * 0.8f;
        float t = dur;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            transform.localScale = Vector3.one * k;
            yield return null;
        }
        if (bg) bg.enabled = false;
        if (icon) icon.enabled = false;
        transform.localScale = Vector3.zero;
        co = null;
    }

    void LateUpdate()
    {
        // mantém o balão posicionado acima da cabeça
        transform.localPosition = offset;
    }
}
