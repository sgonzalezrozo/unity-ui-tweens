using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class TweenBenchmarkMetric
{
    public string name;
    public double value;
    public string unit;
    public string aggregation;
}

[Serializable]
public sealed class TweenBenchmarkSession
{
    public string deviceName;
    public string deviceModel;
    public string osVersion;
    public string unityVersion;
    public string gitCommit;
    public string buildMode;
    public string scriptingBackend;
    public string notes;
}

[Serializable]
public sealed class TweenBenchmarkWorkload
{
    public string profileOrScenario;
    public string variant;
    public int runIndex;
    public int totalRuns;
    public int warmupFrames;
    public int measuredFrames;
    public int seed;
}

[Serializable]
public sealed class TweenBenchmarkResult
{
    public string tool = "ui-tweens";
    public string schemaVersion = "1.0.0";
    public string timestampUtc;
    public TweenBenchmarkSession session;
    public TweenBenchmarkWorkload workload;
    public List<TweenBenchmarkMetric> metrics = new();
}

/// <summary>Writes each result as soon as it's measured, so a crash keeps earlier results.</summary>
public sealed class TweenBenchmarkReport
{
    private readonly string launchTimestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

    public TweenBenchmarkReport()
    {
        Folder = Path.Combine(Application.persistentDataPath, "UITweensBenchmarks");
        Directory.CreateDirectory(Folder);
    }

    public string Folder { get; }

    public int SavedCount { get; private set; }

    public void Save(TweenBenchmarkResult result)
    {
        string scenario = SanitizePathToken(result.workload.profileOrScenario);
        string variant = SanitizePathToken(result.workload.variant);
        string fileName = $"ui-tweens-{launchTimestamp}-{variant}-{scenario}-run{result.workload.runIndex:000}.json";

        File.WriteAllText(Path.Combine(Folder, fileName), JsonUtility.ToJson(result, true));
        SavedCount++;
    }

    public static TweenBenchmarkSession CreateSession(string commitHash, string notes)
    {
        return new TweenBenchmarkSession
        {
            deviceName = BuildAnonymousDeviceId(SystemInfo.deviceName, SystemInfo.deviceModel),
            deviceModel = SystemInfo.deviceModel,
            osVersion = SystemInfo.operatingSystem,
            unityVersion = Application.unityVersion,
            gitCommit = commitHash,
            buildMode = Debug.isDebugBuild ? "Development" : "Release",
#if ENABLE_IL2CPP
            scriptingBackend = "IL2CPP",
#else
            scriptingBackend = "Mono",
#endif
            notes = notes
        };
    }

    /// <summary>Accepts a 7 to 40 character hex hash. Rejects empty and all-zero placeholders.</summary>
    public static bool TryNormalizeCommitHash(string commitHash, out string normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(commitHash))
            return false;

        string trimmed = commitHash.Trim();
        if (trimmed.Length < 7 || trimmed.Length > 40)
            return false;

        bool hasNonZero = false;
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            bool isHex = (c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F');
            if (!isHex)
                return false;

            hasNonZero |= c != '0';
        }

        if (!hasNonZero)
            return false;

        normalized = trimmed.ToLowerInvariant();
        return true;
    }

    private static string SanitizePathToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return "unknown";

        StringBuilder sb = new(token.Length);
        for (int i = 0; i < token.Length; i++)
        {
            char c = token[i];
            sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        return sb.ToString().Trim('-');
    }

    private static string BuildAnonymousDeviceId(string deviceName, string deviceModel)
    {
        string source = $"{deviceName}|{deviceModel}";
        unchecked
        {
            uint hash = 2166136261;
            for (int i = 0; i < source.Length; i++)
            {
                hash ^= source[i];
                hash *= 16777619;
            }

            return $"device-{hash:x8}";
        }
    }
}

/// <summary>
/// Shows progress on the device. The text changes only between scenarios, so it adds nothing
/// to a measured window. IMGUI isn't used because OnGUI would allocate during measurement.
/// </summary>
public sealed class TweenBenchmarkStatus
{
    private readonly Text label;

    public TweenBenchmarkStatus(Transform owner)
    {
        GameObject canvasObject = new("Benchmark Status", typeof(Canvas));
        canvasObject.transform.SetParent(owner, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        GameObject labelObject = new("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(24f, 24f);
        rect.offsetMax = new Vector2(-24f, -24f);

        label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.color = Color.white;
        label.alignment = TextAnchor.UpperLeft;
        label.raycastTarget = false;
    }

    public void Show(string message)
    {
        label.text = message;
    }
}
