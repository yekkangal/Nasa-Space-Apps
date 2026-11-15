// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public string sceneName;
    public string Prompt => "E to Interact";

    public void Interact()
    {
        SceneFader.LoadScene(sceneName);
    }
}