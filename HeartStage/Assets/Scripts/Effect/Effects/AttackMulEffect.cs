using UnityEngine;

public class AttackMulEffect : EffectBase, IStatMulSource
{
    private const int EffectId = 3001; // 🔥 CSV와 맞춰줄 ID

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
        Debug.Log($"[AttackMulEffect] OnApply mag={magnitude}, dur={duration}", this);
    }

    protected override void OnRemove()
    {
        Debug.Log("[AttackMulEffect] OnRemove", this);
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
// 1) 기본 공격력 (나중에 런타임 스탯으로 바꿔도 됨)
//int baseAtk = data.atk_dmg;
//Debug.Log($"CharacterAttack.Fire: baseAtk={baseAtk}");

// 2) 이 캐릭터에 붙어 있는 모든 IStatMulSource들 중
//    Attack에 해당하는 배율을 전부 곱한 값
//float atkMul = StatMultiplier.GetTotalMultiplier(gameObject, StatType.Attack);
// 또는 this.gameObject.GetStatMul(StatType.Attack);

// 3) 최종 대미지 계산
//int finalDmg = Mathf.RoundToInt(baseAtk * atkMul);
//Debug.Log($"CharacterAttack.Fire: baseAtk={baseAtk}, atkMul={atkMul}, finalDmg={finalDmg}");