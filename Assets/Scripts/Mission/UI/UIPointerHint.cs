// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MissionArchitect
{
    /// <summary>Tiny pointer bridge used by the ANALYSIS rows and other hover hints.</summary>
    public sealed class UIPointerHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Hint;
        public Action<string> OnHover;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (OnHover != null) OnHover(Hint);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (OnHover != null) OnHover(null);
        }
    }
}
