using UnityEngine;


public static class EffectRegistry
{
    // value/duration/tickInterval are from SkillTable
    public static void Apply(GameObject target, int effectId, float value, float duration, float tickInterval = 0f)
    {
        switch (effectId)
        {
            case 3001: // Attack% Up
                EffectBase.Add<AttackMulEffect>(target, duration, value); break;
            case 3101: // Blind (hit rate down)
                EffectBase.Add<BlindEffect>(target, duration, value); break;
            case 3201: // Stun
                EffectBase.Add<StunEffect>(target, duration); break;
            default:
                Debug.LogWarning($"[EffectRegistry] Unknown effectId={effectId}"); break;
        }
    }
}
