// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batch-mode Windows player build. Entry point:
/// Unity.exe -batchmode -nographics -quit -projectPath &lt;project&gt;
///   -executeMethod BuildScript.BuildWindows -logFile Builds/build_windows.log
/// </summary>
public static class BuildScript
{
    const string OutputExe = "Builds/Windows/MissionArchitect.exe";
    const string StartScene = "Outdoor";

    public static void BuildWindows()
    {
        ConfigurePlayer();

        var scenes = BuildSceneList();
        if (scenes.Count == 0)
        {
            Debug.LogError("[BuildScript] No enabled scenes in EditorBuildSettings; nothing to build.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("[BuildScript] Scene order: " + string.Join(", ", scenes));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(OutputExe)) ?? "Builds/Windows");

        var options = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = Path.GetFullPath(OutputExe),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[BuildScript] Build succeeded: " + summary.outputPath +
                      " (" + summary.totalSize + " bytes, " + summary.totalTime + ")");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("[BuildScript] Build failed: " + summary.result +
                           " with " + summary.totalErrors + " error(s).");
            EditorApplication.Exit(1);
        }
    }

    static void ConfigurePlayer()
    {
        PlayerSettings.productName = "Mission Architect";
        PlayerSettings.companyName = "Mission Architect Team";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

        // Make sure the editor is on the Windows target before building.
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);
    }

    /// <summary>Enabled build scene paths, with the outdoor/start scene forced to index 0.</summary>
    static List<string> BuildSceneList()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();
        int start = scenes.FindIndex(p =>
            Path.GetFileNameWithoutExtension(p) == StartScene);
        if (start > 0)
        {
            string scene = scenes[start];
            scenes.RemoveAt(start);
            scenes.Insert(0, scene);
            Debug.Log("[BuildScript] Moved " + StartScene + " to the first build index.");
        }
        else if (start < 0)
        {
            Debug.LogWarning("[BuildScript] No scene named " + StartScene +
                             " found; keeping the existing build order.");
        }
        return scenes;
    }
}
