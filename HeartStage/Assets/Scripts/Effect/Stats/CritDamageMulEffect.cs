using UnityEngine;

public class CritDamageMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3007; // 치피 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<CritDamageMulEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
    }

    protected override void OnRemove()
    {
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.CritDamage)
        {
            // mag = 0.5 → 치피 1.5배
            float factor = 1f + magnitude;
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}
