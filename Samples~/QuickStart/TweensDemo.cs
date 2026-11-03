using UITweens.Extensions;
using UITweens.Math;
using UnityEngine;
using UnityEngine.UI;

public class TweensDemo : MonoBehaviour
{
    private const int BoxCount = 5;
    private const float BoxSpacing = 120f;
    private const float StartY = -200f;
    private const float EndY = 100f;
    private const float Duration = 0.6f;
    private const float Stagger = 0.1f;

    private void Start()
    {
        GameObject canvasObject = new("Demo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        for (int i = 0; i < BoxCount; i++)
            CreateBox(canvasObject.transform, i);
    }

    private static void CreateBox(Transform parent, int index)
    {
        GameObject box = new($"Box {index}", typeof(RectTransform), typeof(Image));
        box.transform.SetParent(parent, false);

        float x = (index - (BoxCount - 1) * 0.5f) * BoxSpacing;

        RectTransform rect = box.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 100f);
        rect.anchoredPosition = new Vector2(x, StartY);

        rect.DOAnchorPos(new Vector2(x, EndY), Duration, EaseType.BackOut)
            .Delay(index * Stagger)
            .OnComplete(() => Pulse(rect));
    }

    private static void Pulse(RectTransform rect)
    {
        rect.DOScale(Vector3.one * 1.1f, 0.4f, EaseType.SineInOut).LoopInfinite();
    }
}
