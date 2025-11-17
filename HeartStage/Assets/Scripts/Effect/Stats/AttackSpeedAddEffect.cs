using UnityEngine;

public class AttackSpeedAddEffect : EffectBase, IStatAddSource
{
    private const int EffectId = 3002; // EffectTable의 사거리 증감 ID

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                EffectBase.Add<AttackSpeedAddEffect>(target, duration, value, tick)
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
        if (stat == StatType.AttackSpeed)
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

float baseSpeed = data.atk_speed;
float finalSpeed = StatCalc.GetFinalStat(gameObject, StatType.AttackSpeed, baseSpeed);
*/
