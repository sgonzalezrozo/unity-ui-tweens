using System.Collections.Generic;
using UITweens.Extensions;
using UnityEngine;
using UnityEngine.UI;

public sealed class TweenBenchmarkTargets
{
    private readonly List<RectTransform> rectTargets = new();
    private readonly List<GameObject> rootObjects = new();

    private readonly Transform owner;
    private Canvas sharedCanvas;

    public TweenBenchmarkTargets(Transform owner)
    {
        this.owner = owner;
    }

    public int Count
        => rectTargets.Count;

    public RectTransform GetRect(int index)
        => rectTargets[index];

    public void EnsureCount(int desiredCount, bool separateCanvasPerTarget)
    {
        while (rectTargets.Count < desiredCount)
            CreateTarget(rectTargets.Count, separateCanvasPerTarget);
    }

    public void Rebuild(int desiredCount, bool separateCanvasPerTarget)
    {
        DestroyAll();
        EnsureCount(desiredCount, separateCanvasPerTarget);
    }

    public void CancelAllTweens()
    {
        for (int i = 0; i < rectTargets.Count; i++)
            rectTargets[i].DoCancel();
    }

    public void DestroyAll()
    {
        for (int i = rootObjects.Count - 1; i >= 0; i--)
        {
            if (rootObjects[i] != null)
                Object.Destroy(rootObjects[i]);
        }

        rectTargets.Clear();
        rootObjects.Clear();
        sharedCanvas = null;
    }

    private void CreateTarget(int index, bool separateCanvasPerTarget)
    {
        Canvas canvas = separateCanvasPerTarget ? CreateCanvas($"Benchmark Canvas {index}") : GetOrCreateSharedCanvas();

        GameObject go = new($"Benchmark Target {index}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80f, 80f);
        rect.anchoredPosition = ComputePosition(index);

        Image image = go.GetComponent<Image>();
        image.color = Color.HSVToRGB((index * 0.037f) % 1f, 0.8f, 0.95f);

        rectTargets.Add(rect);
        rootObjects.Add(go);
    }

    private Canvas GetOrCreateSharedCanvas()
    {
        if (sharedCanvas != null)
            return sharedCanvas;

        sharedCanvas = CreateCanvas("Benchmark Shared Canvas");
        return sharedCanvas;
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(owner, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        rootObjects.Add(canvasObject);
        return canvas;
    }

    private static Vector2 ComputePosition(int index)
    {
        const int columns = 24;
        const float spacing = 84f;
        const float startX = -960f + spacing;
        const float startY = 500f;

        int row = index / columns;
        int col = index % columns;
        return new Vector2(startX + col * spacing, startY - row * spacing);
    }
}
