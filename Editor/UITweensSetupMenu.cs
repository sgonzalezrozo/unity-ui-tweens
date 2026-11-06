#if UNITY_EDITOR

using UITweens;
using UnityEditor;
using UnityEngine;

namespace UITweens.Editor
{
    /// <summary>Tells people who run the old setup step that it's no longer needed.</summary>
    public class UITweensSetupMenu
    {
        [MenuItem("Tools/UI Tweens/Setup", priority = 1)]
        public static void ShowSetupInfo()
        {
            EditorUtility.DisplayDialog(
                "UI Tweens Setup",
                "Setup is no longer needed. TweenManager now creates itself when the game starts.\n\n" +
                "You can delete Assets/Resources/Tools/UITweens/TweenManager.prefab.",
                "OK");
        }
    }
}

#endif
