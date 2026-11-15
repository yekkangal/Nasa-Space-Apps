// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;

namespace MissionArchitect
{
    /// <summary>Tiny ring buffer for the terminal event console (last 8 lines).</summary>
    public static class MissionLog
    {
        public const int Capacity = 8;
        static readonly List<string> _lines = new List<string>(Capacity + 1);

        public static IReadOnlyList<string> Lines => _lines;

        public static void Add(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            _lines.Add(line);
            while (_lines.Count > Capacity) _lines.RemoveAt(0);
        }

        public static void Clear() { _lines.Clear(); }

        public static string Join(string separator = "\n")
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _lines.Count; i++)
            {
                if (i > 0) sb.Append(separator);
                sb.Append(_lines[i]);
            }
            return sb.ToString();
        }
    }
}
