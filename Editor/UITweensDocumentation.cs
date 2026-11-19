using UnityEditor;
using UnityEngine;

namespace UITweens.Editor
{
    /// <summary>Links to the UI Tweens documentation: the README on GitHub.</summary>
    internal static class UITweensDocumentation
    {
        internal const string Url = "https://github.com/sgonzalezrozo/unity-ui-tweens#readme";

        [MenuItem("Tools/UI Tweens/Documentation", priority = 100)]
        private static void OpenDocumentation() => Application.OpenURL(Url);
    }
}
