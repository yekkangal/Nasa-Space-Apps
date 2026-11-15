// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEngine;
using UnityEngine.InputSystem;

namespace MissionArchitect
{
    /// <summary>Background music: persistent 2D looping source with fade-in, ducking and mute.</summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        const string ClipPath = "Music/bgm";
        const float BaseVolume = 0.4f;
        const float DuckVolume = 0.25f;
        const float FadeInSeconds = 2.0f;
        const string MuteKey = "MA_MusicMuted";

        static MusicPlayer _instance;
        AudioSource _source;
        bool _muted;
        bool _ready;
        float _fade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (_instance != null) return;
            var go = new GameObject("MusicPlayer");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MusicPlayer>();
            _instance.Build();
        }

        void Build()
        {
            var clip = Resources.Load<AudioClip>(ClipPath);
            if (clip == null)
            {
                Debug.LogWarning("[MusicPlayer] No music clip at Resources/" + ClipPath +
                                 ". Add bgm.mp3, bgm.ogg or bgm.wav later; continuing without music.");
                return;
            }

            _source = gameObject.AddComponent<AudioSource>();
            _source.clip = clip;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.playOnAwake = false;
            _source.volume = 0f;
            _source.Play();
            _ready = true;
            _muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
        }

        void Update()
        {
            if (!_ready) return;

            var kb = Keyboard.current;
            if (kb != null && kb.mKey.wasPressedThisFrame)
            {
                _muted = !_muted;
                PlayerPrefs.SetInt(MuteKey, _muted ? 1 : 0);
                PlayerPrefs.Save();
            }

            _fade = Mathf.MoveTowards(_fade, 1f, Time.unscaledDeltaTime / FadeInSeconds);
            bool duck = MissionUIState.AnyOpen;
            float target = _muted ? 0f : (duck ? DuckVolume : BaseVolume) * _fade;
            _source.volume = Mathf.MoveTowards(_source.volume, target, Time.unscaledDeltaTime * 0.6f);
        }
    }
}
