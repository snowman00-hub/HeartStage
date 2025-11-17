using UnityEngine;

public class IncomingDamageMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3015; // 피해증가(받는 피해 증감)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<IncomingDamageMulEffect>(target, duration, value, tick)
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
        if (stat == StatType.IncomingDamage)
        {
            // mag = +0.25 → 받는 피해 1.25배
            // mag = -0.4  → 받는 피해 0.6배
            float factor = 1f + magnitude;
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}
