using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    public Image fill;                           // arraste o filho Fill
    public Vector3 followOffset = new Vector3(0f, 0.9f, 0f);

    Transform target;

    public void AttachTo(Transform t) { target = t; }

    void LateUpdate()
    {
        if (target) transform.position = target.position + followOffset;
    }

    public void Set01(float v)
    {
        if (fill) fill.fillAmount = Mathf.Clamp01(v);
    }
}
