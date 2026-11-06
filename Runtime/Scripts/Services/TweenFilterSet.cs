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

        public EcsFilter<TweenTargetComponent, TweenClockComponent, TweenControlComponent> Delayed;
        public EcsFilter<TweenClockComponent, TweenAlphaComponent, TweenResultComponent, TweenControlComponent> Alpha;
        public EcsFilter<TweenClockComponent, TweenPositionComponent, TweenResultComponent, TweenControlComponent> Position;
        public EcsFilter<TweenClockComponent, TweenScaleComponent, TweenResultComponent, TweenControlComponent> Scale;
        public EcsFilter<TweenClockComponent, TweenRotationComponent, TweenResultComponent, TweenControlComponent> Rotation;
        public EcsFilter<TweenClockComponent, TweenColorComponent, TweenResultComponent, TweenControlComponent> Color;
        public EcsFilter<TweenClockComponent, TweenSizeDeltaComponent, TweenResultComponent, TweenControlComponent> SizeDelta;
        public EcsFilter<TweenClockComponent, TweenPivotComponent, TweenResultComponent, TweenControlComponent> Pivot;
        public EcsFilter<TweenClockComponent, TweenCustomFloatComponent, TweenControlComponent> CustomFloat;
        public EcsFilter<TweenClockComponent, TweenCustomVectorComponent, TweenControlComponent> CustomVector;

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
