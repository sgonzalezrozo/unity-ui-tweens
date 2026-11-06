using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>
    /// LeoECS filters, created the first time a tween type needs one. Each filter allocates large
    /// arrays and touches the component pools it uses, so unused tween types cost nothing.
    /// </summary>
    /// <remarks>Pooled entities keep their property component, so filters include them. The
    /// systems skip them through their Pooled state, which avoids filter updates on each reuse.</remarks>
    internal sealed class TweenFilterSet
    {
        private readonly EcsWorld world;

        public EcsFilter<TweenCoreComponent> Delayed;
        public EcsFilter<TweenCoreComponent, TweenAlphaComponent> Alpha;
        public EcsFilter<TweenCoreComponent, TweenPositionComponent> Position;
        public EcsFilter<TweenCoreComponent, TweenScaleComponent> Scale;
        public EcsFilter<TweenCoreComponent, TweenRotationComponent> Rotation;
        public EcsFilter<TweenCoreComponent, TweenColorComponent> Color;
        public EcsFilter<TweenCoreComponent, TweenSizeDeltaComponent> SizeDelta;
        public EcsFilter<TweenCoreComponent, TweenPivotComponent> Pivot;
        public EcsFilter<TweenCoreComponent, TweenCustomFloatComponent> CustomFloat;
        public EcsFilter<TweenCoreComponent, TweenCustomVectorComponent> CustomVector;

        // Active tweens of each kind. Systems stop scanning a filter once they've seen them all,
        // so pooled entities in the filters cost almost nothing.
        private readonly int[] activeByKind = new int[10];

        public int ActiveTotal { get; private set; }

        internal void OnActivated(TweenPoolKind kind)
        {
            activeByKind[(int)kind]++;
            ActiveTotal++;
        }

        internal void OnDeactivated(TweenPoolKind kind)
        {
            activeByKind[(int)kind]--;
            ActiveTotal--;
        }

        /// <summary>
        /// Active tweens of one kind. A tween with no kind can carry any property component, so
        /// while one exists, this returns a count that never stops a scan early.
        /// </summary>
        public int GetActiveCount(TweenPoolKind kind)
            => activeByKind[(int)TweenPoolKind.None] > 0 ? int.MaxValue : activeByKind[(int)kind];

        public TweenFilterSet(EcsWorld world)
        {
            this.world = world;
        }

        public void RequireDelayed() => Require(ref Delayed);
        public void RequireAlpha() => Require(ref Alpha);
        public void RequirePosition() => Require(ref Position);
        public void RequireScale() => Require(ref Scale);
        public void RequireRotation() => Require(ref Rotation);
        public void RequireColor() => Require(ref Color);
        public void RequireSizeDelta() => Require(ref SizeDelta);
        public void RequirePivot() => Require(ref Pivot);
        public void RequireCustomFloat() => Require(ref CustomFloat);
        public void RequireCustomVector() => Require(ref CustomVector);

        private void Require<T>(ref T filter) where T : EcsFilter
        {
            if (filter == null)
                filter = (T)world.GetFilter(typeof(T));
        }
    }
}
