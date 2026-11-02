using System.Collections.Generic;
using UnityEngine;

namespace UITweens.Services
{
    public class DelayedTweenQueue
    {
        private List<TweenDefinition> pendingTweens = new();
        private Dictionary<(int, TweenPropertyType), int> componentPropertyToIndex = new();

        public int Count
            => pendingTweens.Count;

        public void Enqueue(TweenDefinition definition)
        {
            if (definition.TargetComponent == null)
            {
                Debug.LogWarning("[UITweens] Cannot enqueue tween with null target component");
                return;
            }

            int componentId = definition.TargetComponent.GetInstanceID();
            (int, TweenPropertyType) key = (componentId, definition.PropertyType);

            if (componentPropertyToIndex.TryGetValue(key, out int existingIndex))
            {
                if (existingIndex >= 0 && existingIndex < pendingTweens.Count)
                {
                    pendingTweens.RemoveAt(existingIndex);
                    RebuildIndexMap();
                }
            }

            int newIndex = pendingTweens.Count;
            pendingTweens.Add(definition);
            componentPropertyToIndex[key] = newIndex;
        }

        public void ProcessDelays(float deltaTime, List<TweenDefinition> readyToSpawn)
        {
            readyToSpawn.Clear();

            for (int i = pendingTweens.Count - 1; i >= 0; i--)
            {
                TweenDefinition def = pendingTweens[i];

                if (def.TargetComponent == null)
                {
                    pendingTweens.RemoveAt(i);
                    continue;
                }

                def.RemainingDelay -= deltaTime;

                if (def.RemainingDelay <= 0f)
                {
                    readyToSpawn.Add(def);
                    pendingTweens.RemoveAt(i);

                    int componentId = def.TargetComponent.GetInstanceID();
                    componentPropertyToIndex.Remove((componentId, def.PropertyType));
                }
                else
                {
                    pendingTweens[i] = def;
                }
            }

            if (readyToSpawn.Count > 0)
                RebuildIndexMap();
        }

        public void RemoveForComponent(UnityEngine.Object component)
        {
            if (component == null)
                return;

            int componentId = component.GetInstanceID();
            bool removed = false;

            for (int i = pendingTweens.Count - 1; i >= 0; i--)
            {
                if (pendingTweens[i].TargetComponent == null || pendingTweens[i].TargetComponent.GetInstanceID() != componentId)
                    continue;

                pendingTweens.RemoveAt(i);
                removed = true;
            }

            if (removed)
                RebuildIndexMap();
        }

        public void Clear()
        {
            pendingTweens.Clear();
            componentPropertyToIndex.Clear();
        }

        private void RebuildIndexMap()
        {
            componentPropertyToIndex.Clear();
            for (int i = 0; i < pendingTweens.Count; i++)
            {
                TweenDefinition def = pendingTweens[i];
                if (def.TargetComponent == null)
                    continue;

                int componentId = def.TargetComponent.GetInstanceID();
                componentPropertyToIndex[(componentId, def.PropertyType)] = i;
            }
        }
    }
}

