using UnityEditor;
using UnityEngine;

namespace UITweens.Editor
{
    /// <summary>
    /// Links to the UI Tweens documentation. After the package is imported, shows a welcome dialog once per project.
    /// </summary>
    [InitializeOnLoad]
    internal static class UITweensDocumentation
    {
        internal const string Url = "https://github.com/sgonzalezrozo/unity-ui-tweens#readme";

        private const string ShownKeyPrefix = "com.sgonzalezrozo.ui-tweens.WelcomeShown:";

        static UITweensDocumentation()
        {
            if (Application.isBatchMode || EditorPrefs.GetBool(ShownKey, false))
                return;

            EditorApplication.update += ShowWelcomeWhenIdle;
        }

        // One key per project, so the dialog shows again in another project.
        private static string ShownKey => ShownKeyPrefix + Application.dataPath;

        private static void ShowWelcomeWhenIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorApplication.update -= ShowWelcomeWhenIdle;
            if (EditorPrefs.GetBool(ShownKey, false))
                return;

            EditorPrefs.SetBool(ShownKey, true);
            bool open = EditorUtility.DisplayDialog(
                "UI Tweens is installed",
                "There is nothing to set up: the first tween starts UI Tweens.\n\nThe documentation shows the tween API, sequences, loops and easings.",
                "Open Documentation",
                "Close");

            if (open)
                OpenDocumentation();
        }

        internal static void OpenDocumentation() => Application.OpenURL(Url);
    }
}
