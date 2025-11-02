using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSfx : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    public AudioSource src;         // arraste o AudioSource do botão
    public AudioClip hoverClip;     // som ao passar o mouse
    public AudioClip clickClip;     // som ao clicar (opcional)
    public float hoverVol = 0.7f;
    public float clickVol = 0.8f;

    void Reset() { src = GetComponent<AudioSource>(); }

    public void OnPointerEnter(PointerEventData e)
    { if (src && hoverClip) src.PlayOneShot(hoverClip, hoverVol); }

    public void OnPointerClick(PointerEventData e)
    { if (src && clickClip) src.PlayOneShot(clickClip, clickVol); }

    // toca também quando o botão recebe foco por teclado/controle
    public void OnSelect(BaseEventData e)
    { if (src && hoverClip) src.PlayOneShot(hoverClip, hoverVol); }
    public void OnDeselect(BaseEventData e) { }
}
