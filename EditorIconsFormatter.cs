#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.EditorIconsUI
{
    // https://gist.githubusercontent.com/MattRix/c1f7840ae2419d8eb2ec0695448d4321/raw/UnityEditorIcons.txt
    public static class EditorIconsFormatter
    {
        [MenuItem("Tools/Format txt to C# Array")]
        public static void FormatClipboard()
        {
            var rawText = EditorGUIUtility.systemCopyBuffer;

            if (string.IsNullOrWhiteSpace(rawText))
            {
                Debug.LogWarning("Clipboard is empty.");
                return;
            }

            var formattedLines = rawText
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .Distinct() // Remove duplicates
                .Select(line => $"\"{line}\"");

            var csharpArrayFormat = string.Join(", ", formattedLines);

            EditorGUIUtility.systemCopyBuffer = csharpArrayFormat;

            Debug.Log("Formatted C# array copied to clipboard! Ready to paste into IcoList.");
        }
    }
}
#endif
