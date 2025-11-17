using UnityEngine;

public class DropAmountMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3009; // 재화 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<DropAmountMulEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
        Debug.Log($"[DropAmountMulEffect] OnApply mag={magnitude}, dur={duration}", this);
    }

    protected override void OnRemove()
    {
        Debug.Log("[DropAmountMulEffect] OnRemove", this);
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.DropAmountRate)
        {
            float factor = 1f + magnitude; // +1.0 → 2배 드랍
            factor = Mathf.Max(0f, factor);
            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}
