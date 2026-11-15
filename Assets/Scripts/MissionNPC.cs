// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections;
using UnityEngine;
using TMPro;

public class MissionNPC : MonoBehaviour, IInteractable
{
    public GameObject dialoguePanel;
    public TMP_Text selectedMissionText;
    public float labelDuration = 3f;    // seconds the label stays on screen
    public float labelFadeTime = 0.4f;  // fade out duration
    public string Prompt => "E - Talk";

    Coroutine labelRoutine;
    Coroutine loadRoutine;

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (selectedMissionText != null)
            selectedMissionText.gameObject.SetActive(false);
    }

    public void Interact()
    {
        dialoguePanel.SetActive(true);
        GameState.InputLocked = true;
    }

    // The mission buttons call this with the baked internal id ("Ay", "Mars", "Asteroit").
    public void SelectMission(string mission)
    {
        GameState.SelectedMission = GameState.NormalizeMission(mission);
        dialoguePanel.SetActive(false);
        GameState.InputLocked = false;
        ShowLabel();

        if (loadRoutine != null) StopCoroutine(loadRoutine);
        loadRoutine = StartCoroutine(LoadMissionLab());
    }

    IEnumerator LoadMissionLab()
    {
        yield return new WaitForSeconds(1.2f);
        SceneFader.LoadScene("MissionLab");
    }

    void ShowLabel()
    {
        if (selectedMissionText == null) return;
        if (labelRoutine != null) StopCoroutine(labelRoutine);
        labelRoutine = StartCoroutine(LabelRoutine());
    }

    IEnumerator LabelRoutine()
    {
        selectedMissionText.text = "Selected mission: " + GameState.DisplayName(GameState.SelectedMission);
        selectedMissionText.alpha = 1f;
        selectedMissionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(labelDuration);

        float t = 0f;
        while (t < labelFadeTime)
        {
            t += Time.deltaTime;
            selectedMissionText.alpha = 1f - Mathf.Clamp01(t / labelFadeTime);
            yield return null;
        }

        selectedMissionText.gameObject.SetActive(false);
        selectedMissionText.alpha = 1f;
        labelRoutine = null;
    }
}
