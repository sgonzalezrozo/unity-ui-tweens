
using System;
using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Represents a tween declaration that hasn't been spawned yet.
    /// Used with DeclareOnly() + Sequential() for declarative sequence building.
    /// Declarative sequence API
    /// </summary>
    public interface ITweenDeclaration
    {
        /// <summary>
        /// Spawn this tween into the ECS world using the provided spawner.
        /// Called internally by Sequential() when ready to execute.
        /// </summary>
        TweenBuilder Spawn(TweenSpawner spawner);
    }

    /// <summary>
    /// A declared tween that stores its builder for deferred execution.
    /// </summary>
    public class DeclaredTween : ITweenDeclaration
    {
        private readonly Func<TweenSpawner, TweenBuilder> spawnFunc;

        public DeclaredTween(Func<TweenSpawner, TweenBuilder> spawnFunc)
        {
            this.spawnFunc = spawnFunc;
        }

        public TweenBuilder Spawn(TweenSpawner spawner)
        {
            return spawnFunc != null ? spawnFunc.Invoke(spawner) : new TweenBuilder(spawner);
        }
    }

    /// <summary>
    /// A held tween for Sequential(). It's a struct, so declaring a tween doesn't allocate.
    /// </summary>
    public readonly struct TweenDeclaration : ITweenDeclaration
    {
        private readonly TweenBuilder builder;

        internal TweenDeclaration(TweenBuilder builder)
        {
            this.builder = builder;
        }

        public TweenBuilder Spawn(TweenSpawner spawner)
            => builder;
    }

    /// <summary>
    /// A wait in a sequence, with no animated properties. It's a struct, so it doesn't allocate.
    /// Usage: spawner.Wait(0.5f)
    /// </summary>
    public readonly struct TweenWait : ITweenDeclaration
    {
        public float Duration { get; }

        public TweenWait(float duration)
        {
            Duration = Mathf.Max(0f, duration);
        }

        public TweenBuilder Spawn(TweenSpawner spawner)
        {
            if (spawner == null)
            {
                Debug.LogError("[UITweens] TweenSpawner is null in TweenWait.Spawn()");
                return new TweenBuilder(spawner);
            }

            return spawner.CustomFloat(0f, 1f, Duration, EaseType.Linear);
        }
    }
}
