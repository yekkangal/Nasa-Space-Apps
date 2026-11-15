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