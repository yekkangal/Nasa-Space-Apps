// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MissionArchitect
{
    /// <summary>
    /// Builds and wires Assets/Scenes/MissionLab.unity from the Interior scene, idempotently.
    /// Runs once automatically when the scene is missing and is also available from
    /// Tools/Mission Architect/Rebuild Mission Lab scene.
    /// </summary>
    [InitializeOnLoad]
    public static class MissionLabSetup
    {
        const string ScenePath = "Assets/Scenes/MissionLab.unity";
        const string PanelSpritePath = "Assets/Sprites/UI/ui_panel.png";
        const string ButtonSpritePath = "Assets/Sprites/UI/ui_button.png";
        const string FontPath = "Assets/Fonts/PixelifySans-Regular.asset";

        static MissionLabSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoSetup;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                AddToBuildSettings();
                return;
            }
            Build(false);
        }

        [MenuItem("Tools/Mission Architect/Rebuild Mission Lab scene")]
        public static void RebuildMenu()
        {
            Build(true);
        }

        static void Build(bool recreate)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[MissionLabSetup] Not building the scene while in play mode.");
                return;
            }

            if (recreate)
            {
                var loaded = SceneManager.GetSceneByPath(ScenePath);
                if (loaded.IsValid() && loaded.isLoaded) EditorSceneManager.CloseScene(loaded, true);
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                    AssetDatabase.DeleteAsset(ScenePath);
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                string source = FindSourceScene();
                if (string.IsNullOrEmpty(source))
                {
                    Debug.LogError("[MissionLabSetup] No source scene found in Assets/Scenes.");
                    return;
                }
                if (!AssetDatabase.CopyAsset(source, ScenePath))
                {
                    Debug.LogError("[MissionLabSetup] Could not copy " + source + " to " + ScenePath);
                    return;
                }
                AssetDatabase.Refresh();
                Debug.Log("[MissionLabSetup] Copied " + source + " -> " + ScenePath);
            }

            string interiorName = "Interior";
            string sourceScene = FindSourceScene();
            if (!string.IsNullOrEmpty(sourceScene))
                interiorName = Path.GetFileNameWithoutExtension(sourceScene);

            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool wasLoaded = scene.IsValid() && scene.isLoaded;
            var previousActive = SceneManager.GetActiveScene();

            if (!wasLoaded)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            SceneManager.SetActiveScene(scene);
            try
            {
                Wire(scene, interiorName);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AddToBuildSettings();
                Debug.Log("[MissionLabSetup] MissionLab scene wired and saved.");
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
                if (previousActive.IsValid() && previousActive.isLoaded)
                    SceneManager.SetActiveScene(previousActive);
            }
        }

        static void Wire(Scene scene, string interiorName)
        {
            // 1. remove the selection NPC (its dialogue panel is hidden, not deleted)
            foreach (var root in scene.GetRootGameObjects())
            {
                var npc = root.GetComponentInChildren<MissionNPC>(true);
                if (npc != null)
                {
                    Object.DestroyImmediate(npc.gameObject);
                    break;
                }
            }

            var dialogue = FindByName(scene, "DialoguePanel");
            if (dialogue != null) dialogue.SetActive(false);

            var selectionLabel = FindByName(scene, "SelectedMissionText");
            if (selectionLabel != null) selectionLabel.SetActive(false);

            // 2. exit door back to the Interior lab (the room has a gap in its bottom wall)
            var door = FindInScene<Door>(scene);
            if (door == null)
            {
                var doorGo = new GameObject("ExitDoor");
                doorGo.transform.position = new Vector3(0f, -5.2f, 0f);
                var col = doorGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.8f, 1.4f);
                door = doorGo.AddComponent<Door>();
            }
            door.sceneName = interiorName;

            // 3. the TERMINAL object near the top wall
            var terminalRoot = FindRoot(scene, "TERMINAL");
            var terminalGo = terminalRoot != null ? terminalRoot : new GameObject("TERMINAL");
            terminalGo.transform.position = new Vector3(0f, 2.4f, 0f);

            var spriteRenderer = GetOrAdd<SpriteRenderer>(terminalGo);
            spriteRenderer.sortingOrder = 5;

            var box = GetOrAdd<BoxCollider2D>(terminalGo);
            box.isTrigger = true;
            box.size = new Vector2(1.6f, 1.6f);

            var terminal = GetOrAdd<MissionTerminal>(terminalGo);
            terminal.spriteRenderer = spriteRenderer;

            // 4. the bootstrap that creates both runtime UIs and the EventSystem
            var bootRoot = FindRoot(scene, "MissionLabBootstrap");
            var bootGo = bootRoot != null ? bootRoot : new GameObject("MissionLabBootstrap");
            bootGo.transform.position = Vector3.zero;

            var boot = GetOrAdd<MissionLabBootstrap>(bootGo);
            boot.panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath);
            boot.buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            boot.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            boot.terminal = terminal;

            if (boot.panelSprite == null || boot.font == null)
                Debug.LogWarning("[MissionLabSetup] A UI sprite or the pixel font was not found; " +
                                 "the UI will fall back to plain images / default TMP font.");
        }

        static void AddToBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < list.Count; i++)
                if (list[i].path == ScenePath) return;

            list.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("File/Save Project");
            Debug.Log("[MissionLabSetup] MissionLab added to build settings.");
        }

        // ---- scene helpers ----------------------------------------------------

        static string FindSourceScene()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            string fallback = null;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]).Replace('\\', '/');
                if (path == ScenePath) continue;
                if (path.EndsWith("/Interior.unity")) return path;
                if (fallback == null) fallback = path;
            }
            return fallback;
        }

        static T FindInScene<T>(Scene scene) where T : Component
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var c = roots[i].GetComponentInChildren<T>(true);
                if (c != null) return c;
            }
            return null;
        }

        static GameObject FindRoot(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                if (roots[i].name == name) return roots[i];
            return null;
        }

        static GameObject FindByName(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < transforms.Length; t++)
                    if (transforms[t].name == name) return transforms[t].gameObject;
            }
            return null;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }
    }
}
