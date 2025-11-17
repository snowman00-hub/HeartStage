using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectRegistry
{
    // (target, value, duration, tickInterval) → 실제 적용
    public delegate void EffectApplier(GameObject target, float value, float duration, float tickInterval);

    private static readonly Dictionary<int, EffectApplier> map = new();

    // 이걸 각 Effect 클래스에서 호출해주게 할 거임
    public static void Register(int effectId, EffectApplier applier)
    {
        map[effectId] = applier;
        // 중복 등록 시 덮어쓰기 (원하면 Warning 찍어도 됨)
    }

    public static void Apply(GameObject target, int effectId, float value, float duration, float tickInterval = 0f)
    {
        if (target == null)
        {
            Debug.LogWarning("[EffectRegistry] target is null");
            return;
        }

        if (map.TryGetValue(effectId, out var applier))
        {
            applier(target, value, duration, tickInterval);
        }
        else
        {
            Debug.LogWarning($"[EffectRegistry] Unknown effectId={effectId}");
        }
    }
}
