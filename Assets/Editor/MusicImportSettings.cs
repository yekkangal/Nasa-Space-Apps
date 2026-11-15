// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEditor;
using UnityEngine;

namespace MissionArchitect
{
    /// <summary>Streaming Vorbis import settings for tracks under Assets/Resources/Music.</summary>
    public sealed class MusicImportSettings : AssetPostprocessor
    {
        const string MusicFolder = "Assets/Resources/Music/";

        void OnPreprocessAudio()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.StartsWith(MusicFolder)) return;

            var importer = assetImporter as AudioImporter;
            if (importer == null) return;

            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.preloadAudioData = false;
            importer.defaultSampleSettings = settings;
        }
    }
}
