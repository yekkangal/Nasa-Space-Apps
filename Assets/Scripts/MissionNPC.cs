// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections;
using UnityEngine;
using TMPro;

public class MissionNPC : MonoBehaviour, IInteractable
{
    public GameObject dialoguePanel;
    public TMP_Text selectedMissionText;
    public float labelDuration = 3f;    // yazının ekranda kalma süresi
    public float labelFadeTime = 0.4f;  // kaybolma süresi
    public string Prompt => "E to Interact";

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

    // Butonlar bunu çağırıyor
    public void SelectMission(string mission)
    {
        GameState.SelectedMission = mission;
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
        selectedMissionText.text = "Seçilen görev: " + GameState.SelectedMission;
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