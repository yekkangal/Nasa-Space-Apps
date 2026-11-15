// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPopup : MonoBehaviour
{
    public float duration = 0.18f;
    public float startScale = 0.6f;
    public int steps = 5;   // 0 = smooth, 5 = stepped pixel-art motion

    CanvasGroup cg;
    Vector3 baseScale;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        if (cg == null) return;
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float k = Mathf.Clamp01(t);
            if (steps > 0) k = Mathf.Floor(k * steps) / steps;
            float e = 1f - Mathf.Pow(1f - k, 3f);
            transform.localScale = baseScale * Mathf.Lerp(startScale, 1f, e);
            cg.alpha = e;
            yield return null;
        }
        transform.localScale = baseScale;
        cg.alpha = 1f;
    }
}
