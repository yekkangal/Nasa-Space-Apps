// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEngine;

public class IdleBob : MonoBehaviour
{
    public float amplitudePixels = 1.5f;  // vertical travel (pixels)
    public float speed = 1f;              // cycles per second
    public float pixelsPerUnit = 16f;

    Vector3 start;

    void Awake() { start = transform.localPosition; }

    void Update()
    {
        float s = (Mathf.Sin(Time.time * speed * Mathf.PI * 2f) + 1f) * 0.5f;
        float px = Mathf.Round(s * amplitudePixels);   // snap to whole pixels
        transform.localPosition = start + Vector3.up * (px / pixelsPerUnit);
    }
}
