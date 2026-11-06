using System;
using Leopotam.Ecs;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Extensions
{
    public readonly struct CancelTweenBuilder
    {
        private readonly TweenHandle handle;

        internal CancelTweenBuilder(TweenHandle handle)
        {
            this.handle = handle;
        }

        public void OnComplete(Action callback)
        {
            if (callback == null)
            {
                Debug.LogWarning("[UITweens] OnComplete callback is null");
                return;
            }

            if (!handle.IsAlive)
            {
                callback.Invoke();
                return;
            }

            ref var cancelCallback = ref handle.Entity.Get<global::UITweens.Components.TweenCancelCallbackComponent>();
            cancelCallback.OnCancelled = callback;
        }
    }

    public static class TweenControlExtensions
    {
        public static void DoPause(this Image image)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Cannot pause: Image is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(image, out TweenHandle handle))
                TweenManager.PauseComponentTweens(image);
            else
                Debug.LogWarning("[UITweens] No active tween to pause on Image: " + image.name);
        }

        public static void DoPlay(this Image image)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Cannot play: Image is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(image, out TweenHandle handle))
                TweenManager.ResumeComponentTweens(image);
            else
                Debug.LogWarning("[UITweens] No paused tween to resume on Image: " + image.name);
        }

        public static CancelTweenBuilder DoCancel(this Image image)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Cannot cancel: Image is null");
                return new CancelTweenBuilder(new TweenHandle());
            }

            if (TweenManager.TryGetComponentTween(image, out TweenHandle handle))
            {
                TweenManager.CancelComponentTweens(image);
                return new CancelTweenBuilder(handle);
            }
            else
            {
                Debug.LogWarning("[UITweens] No active tween to cancel on Image: " + image.name);
                return new CancelTweenBuilder(new TweenHandle());
            }
        }

        public static void DoTogglePause(this Image image)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Cannot toggle pause: Image is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(image, out TweenHandle handle))
                TweenManager.TogglePauseComponentTweens(image);
            else
                Debug.LogWarning("[UITweens] No active tween to toggle on Image: " + image.name);
        }

        public static void DoPause(this RectTransform rect)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] Cannot pause: RectTransform is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(rect, out TweenHandle handle))
                TweenManager.PauseComponentTweens(rect);
            else
                Debug.LogWarning("[UITweens] No active tween to pause on RectTransform: " + rect.name);
        }

        public static void DoPlay(this RectTransform rect)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] Cannot play: RectTransform is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(rect, out TweenHandle handle))
                TweenManager.ResumeComponentTweens(rect);
            else
                Debug.LogWarning("[UITweens] No paused tween to resume on RectTransform: " + rect.name);
        }

        public static CancelTweenBuilder DoCancel(this RectTransform rect)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] Cannot cancel: RectTransform is null");
                return new CancelTweenBuilder(new TweenHandle());
            }


            if (TweenManager.TryGetComponentTween(rect, out TweenHandle handle))
            {
                TweenManager.CancelComponentTweens(rect);
                return new CancelTweenBuilder(handle);
            }

            return new CancelTweenBuilder(new TweenHandle());
        }

        public static void DoTogglePause(this RectTransform rect)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] Cannot toggle pause: RectTransform is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(rect, out TweenHandle handle))
                TweenManager.TogglePauseComponentTweens(rect);
            else
                Debug.LogWarning("[UITweens] No active tween to toggle on RectTransform: " + rect.name);
        }

        public static void DoPause(this CanvasGroup group)
        {
            if (group == null)
            {
                Debug.LogWarning("[UITweens] Cannot pause: CanvasGroup is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(group, out TweenHandle handle))
                TweenManager.PauseComponentTweens(group);
            else
                Debug.LogWarning("[UITweens] No active tween to pause on CanvasGroup: " + group.name);
        }

        public static void DoPlay(this CanvasGroup group)
        {
            if (group == null)
            {
                Debug.LogWarning("[UITweens] Cannot play: CanvasGroup is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(group, out TweenHandle handle))
                TweenManager.ResumeComponentTweens(group);
            else
                Debug.LogWarning("[UITweens] No paused tween to resume on CanvasGroup: " + group.name);
        }

        public static CancelTweenBuilder DoCancel(this CanvasGroup group)
        {
            if (group == null)
            {
                Debug.LogWarning("[UITweens] Cannot cancel: CanvasGroup is null");
                return new CancelTweenBuilder(new TweenHandle());
            }

            if (TweenManager.TryGetComponentTween(group, out TweenHandle handle))
            {
                TweenManager.CancelComponentTweens(group);
                return new CancelTweenBuilder(handle);
            }
            else
            {
                Debug.LogWarning("[UITweens] No active tween to cancel on CanvasGroup: " + group.name);
                return new CancelTweenBuilder(new TweenHandle());
            }
        }

        public static void DoTogglePause(this CanvasGroup group)
        {
            if (group == null)
            {
                Debug.LogWarning("[UITweens] Cannot toggle pause: CanvasGroup is null");
                return;
            }

            if (TweenManager.TryGetComponentTween(group, out TweenHandle handle))
                TweenManager.TogglePauseComponentTweens(group);
            else
                Debug.LogWarning("[UITweens] No active tween to toggle on CanvasGroup: " + group.name);
        }
    }
}
