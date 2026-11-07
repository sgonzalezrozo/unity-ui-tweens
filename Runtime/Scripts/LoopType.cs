namespace UITweens
{
    /// <summary>How a looping tween repeats.</summary>
    public enum LoopType : byte
    {
        /// <summary>Each loop starts again from the beginning.</summary>
        Restart,

        /// <summary>Plays forward, then back, then forward again, retracing the same motion.</summary>
        Yoyo,
    }
}
