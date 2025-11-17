using UnityEngine;

public class MoveSpeedMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3010; // 이속 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<MoveSpeedMulEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
        Debug.Log($"[MoveSpeedMulEffect] OnApply mag={magnitude}, dur={duration}", this);
    }

    protected override void OnRemove()
    {
        Debug.Log("[MoveSpeedMulEffect] OnRemove", this);
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.MoveSpeed)
        {
            float factor = 1f + magnitude; // +0.3 → 이속 1.3배
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}
