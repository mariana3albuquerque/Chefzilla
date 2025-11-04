using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    public float scale = 1.06f;
    public float duration = 0.08f;
    Vector3 baseScale;
    Coroutine anim;

    void Awake() => baseScale = transform.localScale;

    public void OnPointerEnter(PointerEventData e) => AnimateTo(baseScale * scale);
    public void OnPointerExit(PointerEventData e) => AnimateTo(baseScale);
    public void OnSelect(BaseEventData e) => AnimateTo(baseScale * scale);
    public void OnDeselect(BaseEventData e) => AnimateTo(baseScale);

    void AnimateTo(Vector3 target)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Anim(target));
    }
    System.Collections.IEnumerator Anim(Vector3 target)
    {
        var start = transform.localScale; float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }
}
    