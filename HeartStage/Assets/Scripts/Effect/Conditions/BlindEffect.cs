using System.Collections.Generic;
using UnityEngine;

public class BlindEffect : EffectBase
{
    private static readonly Dictionary<GameObject, float> Penalties = new();
    private float applied; // magnitude 스냅샷

    protected override void OnApply()
    {
        applied = Mathf.Max(0f, magnitude); // 예: 0.30 = 명중률 -30%
        if (Penalties.TryGetValue(gameObject, out var cur)) Penalties[gameObject] = cur + applied;
        else Penalties[gameObject] = applied;
    }

    protected override void OnRemove()
    {
        if (!Penalties.TryGetValue(gameObject, out var cur)) return;
        cur -= applied;
        if (cur <= 0f) Penalties.Remove(gameObject);
        else Penalties[gameObject] = cur;
    }

    // 전투/명중 판정 시 사용: 1.0 - penalty 만큼 명중률을 깎거나, penalty 확률로 빗맞음 처리 등
    public static float GetAccuracyPenalty(GameObject target)
        => Penalties.TryGetValue(target, out var p) ? Mathf.Clamp01(p) : 0f;
}

// 사용 예시:
//float penalty = BlindEffect.GetAccuracyPenalty(attacker);
//float hitChance = baseHitChance * (1f - penalty);