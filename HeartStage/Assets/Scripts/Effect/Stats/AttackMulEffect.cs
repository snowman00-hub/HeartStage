using UnityEngine;

public class AttackMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3001; // CSV와 맞춰줄 ID

    // Unity가 런타임 시작할 때 자동으로 호출해주는 함수
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                EffectBase.Add<AttackMulEffect>(target, duration, value, tick)
        );
    }

    // ====== 기존 구현들 ======
    protected override void OnApply()
    {
    }

    protected override void OnRemove()
    {
    }

    public bool TryGetMul(StatType stat, out float mul)
    {
        if (stat == StatType.Attack)
        {
            // magnitude = +0.15  → ×1.15 (버프)
            // magnitude = -0.30 → ×0.70 (디버프)
            float factor = 1f + magnitude;

            // 0 아래로 내려가면 이상하니까 안전장치만 하나
            factor = Mathf.Max(0f, factor);

            mul = factor;
            return true;
        }

        mul = 1f;
        return false;
    }
}

/*
사용 예시 (CharacterAttack 등에서):

int baseAtk = data.atk_dmg;
float finalAtk = StatCalc.GetFinalStat(gameObject, StatType.Attack, baseAtk);
// 또는 단순 배율만 필요하면:
// float atkMul = gameObject.GetStatMul(StatType.Attack);
*/