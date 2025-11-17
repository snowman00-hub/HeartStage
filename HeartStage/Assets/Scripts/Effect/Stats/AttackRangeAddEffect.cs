using UnityEngine;

/// <summary>
/// 사거리 고정 증가/감소 버프
/// - magnitude = +1.5 → 사거리 +1.5
/// - magnitude = -0.5 → 사거리 -0.5
/// </summary>
public class AttackRangeAddEffect : EffectBase, IStatAddSource
{
    private const int EffectId = 3003; // EffectTable의 사거리 증감 ID

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                EffectBase.Add<AttackRangeAddEffect>(target, duration, value, tick)
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
        if (stat == StatType.AttackRange)
        {
            add = magnitude;
            return true;
        }

        add = 0f;
        return false;
    }
}

/*
사용 예시:

float baseRange = data.atk_range;
float finalRange = StatCalc.GetFinalStat(gameObject, StatType.AttackRange, baseRange);
*/
