using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    public RectTransform promptBox;   // PromptBox
    public TMP_Text promptText;       // içindeki yazı
    public float yOffset = 0.3f;

    IInteractable current;
    Collider2D currentCol;
    Camera cam;

    void OnTriggerEnter2D(Collider2D other)
    {
        var i = other.GetComponent<IInteractable>();
        if (i != null) { current = i; currentCol = other; }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var i = other.GetComponent<IInteractable>();
        if (i != null && i == current) { current = null; currentCol = null; }
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        bool show = current != null && !GameState.InputLocked;

        if (promptBox != null)
        {
            promptBox.gameObject.SetActive(show);
            if (show)
            {
                promptText.text = current.Prompt;
                LayoutRebuilder.ForceRebuildLayoutImmediate(promptBox);

                Bounds b = currentCol.bounds;
                Vector3 world = new Vector3(b.center.x, b.max.y + yOffset, 0f);
                Vector3 sp = cam.WorldToScreenPoint(world);
                promptBox.position = new Vector3(Mathf.Round(sp.x), Mathf.Round(sp.y), 0f);
            }
        }

        if (show && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            current.Interact();
    }
}