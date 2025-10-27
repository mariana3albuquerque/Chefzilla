using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int offset = 0;
    SpriteRenderer sr;
    void Awake() { sr = GetComponent<SpriteRenderer>(); }
    void LateUpdate()
    {
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100) + offset;
    }
}

