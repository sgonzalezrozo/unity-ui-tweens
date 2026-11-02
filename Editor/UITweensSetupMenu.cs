#if UNITY_EDITOR

using UITweens;
using UnityEditor;
using UnityEngine;

namespace UITweens.Editor
{
    /// <summary>
    /// Setup menu for UITweens system.
    /// Provides prefab creation for persistent TweenManager.
    ///
    /// ARCHITECTURE:
    /// - Create prefab once (one-time setup)
    /// - On Play: prefab instantiates with DontDestroyOnLoad
    /// - System persists across scenes, handles cleanup automatically
    ///
    /// This matches the ProjectContext pattern: global scope, persistent, scene-aware cleanup.
    /// </summary>
    public class UITweensSetupMenu
    {
        private const string PREFAB_PATH = "Assets/Resources/Tools/UITweens/TweenManager.prefab";

        /// <summary>
        /// Creates the persistent TweenManager prefab used by UITweens runtime bootstrapping.
        /// </summary>
        [MenuItem("Tools/UI Tweens/Setup", priority = 1)]
        public static void CreatePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null)
            {
                EditorUtility.DisplayDialog(
                    "UI Tweens Setup",
                    $"TweenManager prefab already exists at:\n{PREFAB_PATH}",
                    "OK");
                return;
            }

            string resourcesDir = "Assets/Resources/Tools/UITweens";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                if (!AssetDatabase.IsValidFolder("Assets/Resources/Tools"))
                    AssetDatabase.CreateFolder("Assets/Resources", "Tools");

                AssetDatabase.CreateFolder("Assets/Resources/Tools", "UITweens");
            }

            GameObject tweenManagerGO = new("TweenManager");
            tweenManagerGO.AddComponent<TweenManager>();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tweenManagerGO, PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(tweenManagerGO);

            if (savedPrefab == null)
            {
                Debug.LogError($"[UITweens] Failed to create prefab at {PREFAB_PATH}");
                return;
            }

            AssetDatabase.Refresh();

            Debug.Log($"[UITweens] Prefab created at {PREFAB_PATH}");

            EditorUtility.DisplayDialog(
                "UITweens Prefab Created",
                $"TweenManager prefab created at:\n{PREFAB_PATH}\n\n" +
                "It loads automatically when the game starts, so you don't need to add it to a scene.",
                "OK");
        }

    }
}

#endif
