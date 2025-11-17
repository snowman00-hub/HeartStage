using UnityEngine;

public class MaxHpMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3005; // 체력 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<MaxHpMulEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
        Debug.Log($"[MaxHpMulEffect] OnApply mag={magnitude}, dur={duration}", this);
    }

    protected override void OnRemove()
    {
        Debug.Log("[MaxHpMulEffect] OnRemove", this);
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.MaxHp)
        {
            float factor = 1f + magnitude; // +0.3 → 체력 1.3배
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}

