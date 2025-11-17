using UnityEngine;

public class CritChanceAddEffect : EffectBase, IStatAddSource
{
    private const int EffectId = 3006; // 치확 증감

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<CritChanceAddEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
    }

    protected override void OnRemove()
    {
    }

    public bool TryGetAdd(StatType stat, out float add)
    {
        if (stat == StatType.CritChance)
        {
            // mag = 0.1 → 치확 +10% (최종은 StatCalc에서 0~1로 clamp)
            add = magnitude;
            return true;
        }

        add = 0f;
        return false;
    }
}

