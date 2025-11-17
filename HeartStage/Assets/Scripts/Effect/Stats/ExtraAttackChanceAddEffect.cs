using UnityEngine;

public class ExtraAttackChanceAddEffect : EffectBase, IStatAddSource
{
    private const int EffectId = 3004; // 추가 공격 확률

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<ExtraAttackChanceAddEffect>(target, duration, value, tick)
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
        if (stat == StatType.ExtraAttackChance)
        {
            // mag = 0.2 → 추가공격 확률 +20%
            add = magnitude;
            return true;
        }

        add = 0f;
        return false;
    }
}
