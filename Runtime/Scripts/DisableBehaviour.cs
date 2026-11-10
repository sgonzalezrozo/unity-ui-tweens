namespace UITweens
{
    /// <summary>What a tween does when its GameObject is disabled.</summary>
    public enum DisableBehaviour : byte
    {
        /// <summary>Keeps playing, as if nothing happened. The default.</summary>
        KeepPlaying,

        /// <summary>Pauses while the GameObject is disabled, and carries on when it's enabled again.</summary>
        Pause,

        /// <summary>Cancels the tween, leaving the value where it was.</summary>
        Cancel,

        /// <summary>Jumps to the end and runs OnComplete.</summary>
        Complete,
    }
}
