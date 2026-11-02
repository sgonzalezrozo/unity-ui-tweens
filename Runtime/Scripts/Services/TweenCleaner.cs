using System.Collections.Generic;
using UnityEngine;

namespace UITweens.Services
{
    public class TweenCleaner
    {
        private Dictionary<int, RectTransform> trackedTransforms = new();
        private Dictionary<int, CanvasGroup> trackedCanvasGroups = new();
        private Dictionary<int, UnityEngine.UI.Image> trackedImages = new();
        private List<int> deadTransformIds = new();
        private List<int> deadCanvasGroupIds = new();
        private List<int> deadImageIds = new();

        public void RegisterTransform(RectTransform rectTransform)
        {
            if (rectTransform != null)
                trackedTransforms[rectTransform.GetInstanceID()] = rectTransform;
        }

        public void RegisterCanvasGroup(CanvasGroup canvasGroup)
        {
            if (canvasGroup != null)
                trackedCanvasGroups[canvasGroup.GetInstanceID()] = canvasGroup;
        }

        public void RegisterImage(UnityEngine.UI.Image image)
        {
            if (image != null)
                trackedImages[image.GetInstanceID()] = image;
        }

        public void CleanupOrphanedTweens(TweenTracker tracker)
        {
            deadTransformIds.Clear();
            deadCanvasGroupIds.Clear();
            deadImageIds.Clear();

            foreach (KeyValuePair<int, RectTransform> kvp in trackedTransforms)
            {
                if (kvp.Value == null)
                    deadTransformIds.Add(kvp.Key);
            }

            foreach (KeyValuePair<int, CanvasGroup> kvp in trackedCanvasGroups)
            {
                if (kvp.Value == null)
                    deadCanvasGroupIds.Add(kvp.Key);
            }

            foreach (KeyValuePair<int, UnityEngine.UI.Image> kvp in trackedImages)
            {
                if (kvp.Value == null)
                    deadImageIds.Add(kvp.Key);
            }

            for (int i = 0; i < deadTransformIds.Count; i++)
            {
                tracker.CancelAllTweensForComponent(deadTransformIds[i]);
                trackedTransforms.Remove(deadTransformIds[i]);
            }

            for (int i = 0; i < deadCanvasGroupIds.Count; i++)
            {
                tracker.CancelAllTweensForComponent(deadCanvasGroupIds[i]);
                trackedCanvasGroups.Remove(deadCanvasGroupIds[i]);
            }

            for (int i = 0; i < deadImageIds.Count; i++)
            {
                tracker.CancelAllTweensForComponent(deadImageIds[i]);
                trackedImages.Remove(deadImageIds[i]);
            }
        }
    }
}

