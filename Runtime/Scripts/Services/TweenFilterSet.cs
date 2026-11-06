using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>
    /// LeoECS filters, created the first time a tween type needs one. Each filter allocates large
    /// arrays and touches the component pools it uses, so unused tween types cost nothing.
    /// </summary>
    internal sealed class TweenFilterSet
    {
        private readonly EcsWorld world;

        public EcsFilter<TweenTargetComponent, TweenClockComponent, TweenControlComponent> Delayed;
        public EcsFilter<TweenClockComponent, TweenAlphaComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Alpha;
        public EcsFilter<TweenClockComponent, TweenPositionComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Position;
        public EcsFilter<TweenClockComponent, TweenScaleComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Scale;
        public EcsFilter<TweenClockComponent, TweenRotationComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Rotation;
        public EcsFilter<TweenClockComponent, TweenColorComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Color;
        public EcsFilter<TweenClockComponent, TweenSizeDeltaComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> SizeDelta;
        public EcsFilter<TweenClockComponent, TweenPivotComponent, TweenResultComponent, TweenControlComponent, TweenActiveMarkerComponent> Pivot;
        public EcsFilter<TweenClockComponent, TweenCustomFloatComponent, TweenControlComponent, TweenActiveMarkerComponent> CustomFloat;
        public EcsFilter<TweenClockComponent, TweenCustomVectorComponent, TweenControlComponent, TweenActiveMarkerComponent> CustomVector;

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
