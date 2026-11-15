// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    const float FadeTime = 0.4f;   // half-transition duration
    const int Steps = 6;           // 0 = smooth, 6 = stepped pixel-art fade

    static SceneFader instance;
    CanvasGroup group;
    bool busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        var go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();
        instance.Build();
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;                 // game opens from black
        group.blocksRaycasts = false;
        group.interactable = false;

        var imgGO = new GameObject("Black");
        imgGO.transform.SetParent(transform, false);
        var img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void Start()
    {
        StartCoroutine(Fade(0f));
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }
        if (instance.busy) return;
        instance.StartCoroutine(instance.Transition(sceneName));
    }

    IEnumerator Transition(string sceneName)
    {
        busy = true;
        GameState.InputLocked = true;

        yield return Fade(1f);

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        GameState.InputLocked = false;
        yield return Fade(0f);
        busy = false;
    }

    IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < FadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / FadeTime);
            if (Steps > 0) k = Mathf.Round(k * Steps) / Steps;
            group.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        group.alpha = target;
    }
}
