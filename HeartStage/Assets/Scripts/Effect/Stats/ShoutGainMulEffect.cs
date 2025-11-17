using UnityEngine;

public class ShoutGainMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3008; // 함성 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<ShoutGainMulEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
        Debug.Log($"[ShoutGainMulEffect] OnApply mag={magnitude}, dur={duration}", this);
    }

    protected override void OnRemove()
    {
        Debug.Log("[ShoutGainMulEffect] OnRemove", this);
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.ShoutGainRate)
        {
            float factor = 1f + magnitude; // +0.5 → 1.5배 빨리 참
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}
