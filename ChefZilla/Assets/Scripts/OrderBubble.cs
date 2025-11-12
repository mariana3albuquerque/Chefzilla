using UnityEngine;
using System.Collections;

public class OrderBubble : MonoBehaviour
{
    public SpriteRenderer bg;     // SR do próprio OrderBubble (fundo)
    public SpriteRenderer icon;   // SR do filho Icon (sprite do item)
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);
    public float popDuration = 0.18f;

    Coroutine co;

    void Awake()
    {
        transform.localScale = Vector3.zero;
        if (bg) bg.enabled = false;
        if (icon) icon.enabled = false;
    }

    public void Show(Sprite itemIcon, float delay = 0f)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowCo(itemIcon, delay));
    }

    IEnumerator ShowCo(Sprite itemIcon, float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (icon) icon.sprite = itemIcon;
        if (bg) bg.enabled = true;
        if (icon) icon.enabled = true;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float k = t / popDuration;
            transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.1f, k);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public void Hide()
    {
        if (co != null) StopCoroutine(co);
        StartCoroutine(HideCo());
    }

    IEnumerator HideCo()
    {
        float t = popDuration * 0.8f;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            float k = t / (popDuration * 0.8f);
            transform.localScale = Vector3.one * Mathf.Clamp01(k);
            yield return null;
        }
        if (bg) bg.enabled = false;
        if (icon) icon.enabled = false;
    }

    void LateUpdate()
    {
        // gruda acima da cabeça mesmo com animações
        transform.localPosition = offset;
    }
}
