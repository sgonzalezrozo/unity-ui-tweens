using System.Collections.Generic;
using UnityEngine;

namespace UITweens.Services
{
    public class TweenCleaner
    {
        private readonly Dictionary<int, Object> trackedObjects = new();
        private readonly List<int> deadObjectIds = new();

        public void RegisterTransform(RectTransform rectTransform)
            => Register(rectTransform);

        public void RegisterCanvasGroup(CanvasGroup canvasGroup)
            => Register(canvasGroup);

        public void RegisterImage(UnityEngine.UI.Image image)
            => Register(image);

        public void CleanupOrphanedTweens(TweenTracker tracker)
        {
            deadObjectIds.Clear();

            foreach (KeyValuePair<int, Object> kvp in trackedObjects)
            {
                if (kvp.Value == null)
                    deadObjectIds.Add(kvp.Key);
            }

            for (int i = 0; i < deadObjectIds.Count; i++)
            {
                tracker.CancelAllTweensForComponent(deadObjectIds[i]);
                trackedObjects.Remove(deadObjectIds[i]);
            }
        }

        private void Register(Object target)
        {
            if (target != null)
                trackedObjects[target.GetInstanceID()] = target;
        }
    }
}
