// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEngine;

namespace MissionArchitect
{
    /// <summary>
    /// The interactable lab terminal. Builds its own pixel-art monitor sprite at runtime if
    /// the SpriteRenderer has none, and opens the terminal UI on E.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MissionTerminal : MonoBehaviour, IInteractable
    {
        public SpriteRenderer spriteRenderer;
        [System.NonSerialized] public IMissionTerminalUI terminalUI;

        public string Prompt => "E - Open Terminal";

        void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null) spriteRenderer.sprite = BuildMonitorSprite();
            spriteRenderer.sortingOrder = 5;

            var col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (col.size == Vector2.zero) col.size = new Vector2(1.6f, 1.6f);
        }

        public void Bind(IMissionTerminalUI ui)
        {
            terminalUI = ui;
        }

        public void Interact()
        {
            if (terminalUI == null)
            {
                if (MissionTerminalUI.Instance != null) terminalUI = MissionTerminalUI.Instance;
                else if (MarsTerminalUI.Instance != null) terminalUI = MarsTerminalUI.Instance;
                else if (AsteroidTerminalUI.Instance != null) terminalUI = AsteroidTerminalUI.Instance;
            }
            if (terminalUI != null) terminalUI.Open();
        }

        static Sprite BuildMonitorSprite()
        {
            const int w = 32;
            const int h = 28;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var px = new Color32[w * h];
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 bezel = new Color32(30, 42, 36, 255);
            Color32 bezelHi = new Color32(70, 90, 76, 255);
            Color32 screen = new Color32(8, 18, 13, 255);
            Color32 scan = new Color32(14, 30, 22, 255);
            Color32 glow = new Color32(125, 255, 178, 255);
            Color32 amber = new Color32(255, 200, 87, 255);

            for (int i = 0; i < px.Length; i++) px[i] = clear;

            // stand
            for (int y = 0; y < 4; y++)
                for (int x = 13; x < 19; x++)
                    px[y * w + x] = bezel;
            for (int y = 0; y < 2; y++)
                for (int x = 9; x < 23; x++)
                    px[y * w + x] = bezelHi;

            // body
            for (int y = 4; y < h; y++)
                for (int x = 2; x < w - 2; x++)
                    px[y * w + x] = bezel;

            // top highlight and bottom shade
            for (int x = 3; x < w - 3; x++)
            {
                px[(h - 1) * w + x] = bezelHi;
                px[4 * w + x] = bezelHi;
            }

            // screen
            for (int y = 7; y < h - 3; y++)
                for (int x = 5; x < w - 5; x++)
                    px[y * w + x] = screen;

            // scanlines
            for (int y = 9; y < h - 3; y += 3)
                for (int x = 5; x < w - 5; x++)
                    px[y * w + x] = scan;

            // terminal text lines
            for (int x = 7; x < 25; x++) px[15 * w + x] = glow;
            for (int x = 7; x < 20; x++) px[18 * w + x] = amber;
            for (int x = 7; x < 23; x++) px[21 * w + x] = glow;

            // blinking cursor block
            for (int y = 17; y < 20; y++)
                for (int x = 24; x < 27; x++)
                    px[y * w + x] = amber;

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
