// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>
    /// One-shot, idempotent polish pass for the hand-built scenes:
    /// - translates the baked dialogue / mission buttons and enlarges their text,
    /// - fixes the stale prompt label and the PlayerInteractor prompt wiring,
    /// - adds the Interior exit door that leads back outside,
    /// - checks that the Mission Lab exit door returns to the Interior.
    /// Runs after every compilation (never in play mode) and from Tools/Mission Architect/Apply polish.
    /// </summary>
    public static class PolishSetup
    {
        const string InteriorPath = "Assets/Scenes/Interior.unity";
        const string MissionLabPath = "Assets/Scenes/MissionLab.unity";
        const string FontPath = "Assets/Fonts/PixelifySans-Regular.asset";

        const string NpcLine = "Hi, I'm Mark! Which mission will you take on today?";
        const string MoonLabel = "Moon Mission";
        const string MarsLabel = "Mars Mission";
        const string AsteroidLabel = "Asteroid Mission";
        const string SelectedLabel = "Selected mission: Moon";
        const string ExitPrompt = "E - Exit";

        const float ExitX = 0f;
        const float ExitY = -5.7f;
        const float ExitWidth = 2.8f;
        const float ExitHeight = 1.6f;

        [InitializeOnLoadMethod]
        static void AutoRun()
        {
            EditorApplication.delayCall += TryAutoApply;
        }

        [MenuItem("Tools/Mission Architect/Apply polish")]
        public static void ApplyFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[PolishSetup] Stop play mode before applying scene polish.");
                return;
            }
            Apply(true);
        }

        static void TryAutoApply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryAutoApply;
                return;
            }
            if (BuildPipeline.isBuildingPlayer) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            Apply(false);
        }

        static void Apply(bool fromMenu)
        {
            if (!File.Exists(InteriorPath))
            {
                Debug.LogWarning("[PolishSetup] " + InteriorPath + " not found; scene polish skipped.");
                return;
            }

            string outdoor = FindOutdoorScene();
            bool interiorChanged = PolishScene(InteriorPath, true, outdoor);
            bool labChanged = File.Exists(MissionLabPath) && PolishScene(MissionLabPath, false, outdoor);
            VerifyMissionLab(outdoor);

            if (fromMenu || interiorChanged || labChanged)
            {
                Debug.Log("[PolishSetup] Polish applied. Interior changed: " + interiorChanged +
                          ", MissionLab changed: " + labChanged + ", outdoor scene: " + (outdoor ?? "<not found>"));
            }
        }

        /// <summary>Opens the scene only if needed, edits it, saves it and restores what was open.</summary>
        static bool PolishScene(string path, bool addExitDoor, string outdoor)
        {
            var previousActive = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(path);
            bool wasLoaded = scene.IsValid() && scene.isLoaded;
            bool changed = false;

            try
            {
                if (!wasLoaded)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    if (!scene.IsValid()) return false;
                }

                if (scene.IsValid()) SceneManager.SetActiveScene(scene);

                changed |= PolishTexts(scene);
                changed |= EnsurePromptWiring(scene);
                if (addExitDoor) changed |= EnsureExitDoor(scene, outdoor);
                else changed |= FixLabExitDoor(scene);

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[PolishSetup] Could not polish " + path + ": " + e.Message);
            }
            finally
            {
                if (wasLoaded)
                {
                    if (previousActive.IsValid() && previousActive.isLoaded)
                        SceneManager.SetActiveScene(previousActive);
                }
                else if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return changed;
        }

        // ---- text -------------------------------------------------------------

        static bool PolishTexts(Scene scene)
        {
            bool changed = false;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            foreach (var text in FindAll<TMP_Text>(scene))
            {
                string value = (text.text ?? string.Empty).Trim();

                if (value.Contains("Merhaba") || value.Contains("hangi g"))
                {
                    text.text = NpcLine;
                    ApplySize(text, font, 40f, 32f);
                    Resize(text.rectTransform, 540f, 160f);
                    changed = true;
                    continue;
                }

                if (value == "Ay G" + "\u00F6" + "revi")
                {
                    TranslateButton(text, font, MoonLabel);
                    changed = true;
                    continue;
                }

                if (value == "Mars G" + "\u00F6" + "revi")
                {
                    TranslateButton(text, font, MarsLabel);
                    changed = true;
                    continue;
                }

                if (value == "Asteroit G" + "\u00F6" + "revi")
                {
                    TranslateButton(text, font, AsteroidLabel);
                    changed = true;
                    continue;
                }

                if (value == "E to Interact")
                {
                    // This object is also PlayerInteractor.promptText, so clear the baked label
                    // instead of deleting it (deleting would break the runtime prompt).
                    text.text = string.Empty;
                    text.gameObject.name = "PromptText";
                    ApplySize(text, font, 28f, 24f);
                    changed = true;
                    continue;
                }

                if (value == "New Text")
                {
                    text.text = SelectedLabel;
                    ApplySize(text, font, 32f, 24f);
                    Resize(text.rectTransform, 420f, 60f);
                    changed = true;
                    continue;
                }
            }

            foreach (var panel in FindAll<Image>(scene))
            {
                if (panel.gameObject.name == "DialoguePanel" && panel.color.a < 0.9f)
                {
                    var c = panel.color;
                    c.a = 0.95f;
                    panel.color = c;
                    changed = true;
                }
            }

            return changed;
        }

        static void TranslateButton(TMP_Text label, TMP_FontAsset font, string english)
        {
            label.text = english;
            ApplySize(label, font, 36f, 28f);

            var button = label.GetComponentInParent<Button>();
            if (button != null)
            {
                var buttonRect = button.transform as RectTransform;
                if (buttonRect != null) buttonRect.sizeDelta = new Vector2(380f, 60f);

                var labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(12f, 6f);
                labelRect.offsetMax = new Vector2(-12f, -6f);
            }
            else
            {
                Resize(label.rectTransform, 380f, 56f);
            }
        }

        static void ApplySize(TMP_Text text, TMP_FontAsset font, float maxSize, float minSize)
        {
            if (font != null) text.font = font;
            text.enableAutoSizing = true;
            text.fontSizeMax = maxSize;
            text.fontSizeMin = minSize;
            text.fontSize = maxSize;
            text.lineSpacing = 8f;
            text.SetAllDirty();
        }

        static void Resize(RectTransform rt, float width, float height)
        {
            if (rt == null) return;
            if (rt.anchorMin == rt.anchorMax) rt.sizeDelta = new Vector2(width, height);
        }

        // ---- prompts ----------------------------------------------------------

        static bool EnsurePromptWiring(Scene scene)
        {
            var players = FindAll<PlayerInteractor>(scene);
            if (players.Length == 0) return false;
            var player = players[0];

            bool changed = false;
            if (player.promptBox == null)
            {
                var box = FindInScene<RectTransform>(scene, "PromptBox");
                if (box != null)
                {
                    player.promptBox = box;
                    changed = true;
                }
            }
            if (player.promptText == null)
            {
                var label = FindInScene<TMP_Text>(scene, "PromptText");
                if (label == null) label = FindInScene<TMP_Text>(scene, "PropmtText");
                if (label != null)
                {
                    player.promptText = label;
                    changed = true;
                }
            }
            if (changed) EditorUtility.SetDirty(player);
            return changed;
        }

        // ---- doors ------------------------------------------------------------

        static bool EnsureExitDoor(Scene scene, string outdoor)
        {
            if (string.IsNullOrEmpty(outdoor)) return false;

            bool changed = false;
            Door door = null;
            foreach (var candidate in FindAll<Door>(scene))
            {
                if (candidate.gameObject.name == "ExitDoor")
                {
                    door = candidate;
                    break;
                }
            }

            if (door == null)
            {
                var go = new GameObject("ExitDoor");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.transform.position = new Vector3(ExitX, ExitY, 0f);
                door = go.AddComponent<Door>();
                changed = true;
            }

            var collider = door.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = door.gameObject.AddComponent<BoxCollider2D>();
                changed = true;
            }

            if (door.transform.position != new Vector3(ExitX, ExitY, 0f))
            {
                door.transform.position = new Vector3(ExitX, ExitY, 0f);
                changed = true;
            }

            if (!collider.isTrigger)
            {
                collider.isTrigger = true;
                changed = true;
            }
            if (collider.size != new Vector2(ExitWidth, ExitHeight))
            {
                collider.size = new Vector2(ExitWidth, ExitHeight);
                collider.offset = Vector2.zero;
                changed = true;
            }

            if (door.sceneName != outdoor)
            {
                door.sceneName = outdoor;
                changed = true;
            }
            if (door.promptText != ExitPrompt)
            {
                door.promptText = ExitPrompt;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(door);
                EditorUtility.SetDirty(collider);
            }
            return changed;
        }

        /// <summary>The lab door already exists; only its prompt needs the English label.</summary>
        static bool FixLabExitDoor(Scene scene)
        {
            bool changed = false;
            foreach (var door in FindAll<Door>(scene))
            {
                if (door.promptText != ExitPrompt)
                {
                    door.promptText = ExitPrompt;
                    EditorUtility.SetDirty(door);
                    changed = true;
                }
            }
            return changed;
        }

        /// <summary>The outdoor scene is the build scene whose Door points at the Interior.</summary>
        static string FindOutdoorScene()
        {
            string fallback = null;
            foreach (var entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled || string.IsNullOrEmpty(entry.path)) continue;
                string name = Path.GetFileNameWithoutExtension(entry.path);
                if (name == "Outdoor") fallback = name;
                if (entry.path == InteriorPath || entry.path == MissionLabPath) continue;
                if (!File.Exists(entry.path)) continue;

                try
                {
                    string yaml = File.ReadAllText(entry.path);
                    if (Regex.IsMatch(yaml, @"sceneName:\s*Interior\b")) return name;
                }
                catch { /* unreadable scene: keep looking */ }
            }
            return fallback;
        }

        static void VerifyMissionLab(string outdoor)
        {
            if (!File.Exists(MissionLabPath) || string.IsNullOrEmpty(outdoor)) return;
            string yaml = File.ReadAllText(MissionLabPath);
            if (Regex.IsMatch(yaml, @"sceneName:\s*Interior\b"))
                Debug.Log("[PolishSetup] MissionLab exit door returns to Interior: OK.");
            else
                Debug.LogWarning("[PolishSetup] MissionLab has no door back to Interior; check the lab exit.");
        }

        // ---- helpers ----------------------------------------------------------

        static T[] FindAll<T>(Scene scene) where T : Component
        {
            var found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var list = new List<T>();
            foreach (var item in found)
                if (item != null && item.gameObject.scene == scene) list.Add(item);
            return list.ToArray();
        }

        static T FindInScene<T>(Scene scene, string name) where T : Component
        {
            foreach (var item in FindAll<T>(scene))
                if (item.gameObject.name == name) return item;
            return null;
        }
    }
}
