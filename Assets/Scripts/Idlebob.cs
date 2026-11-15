using UnityEngine;

public class IdleBob : MonoBehaviour
{
    public float amplitudePixels = 1.5f;  // yukarı çıkış (piksel)
    public float speed = 1f;              // saniyedeki tur sayısı
    public float pixelsPerUnit = 16f;

    Vector3 start;

    void Awake() { start = transform.localPosition; }

    void Update()
    {
        float s = (Mathf.Sin(Time.time * speed * Mathf.PI * 2f) + 1f) * 0.5f;
        float px = Mathf.Round(s * amplitudePixels);   // piksel piksel adımlar
        transform.localPosition = start + Vector3.up * (px / pixelsPerUnit);
    }
}