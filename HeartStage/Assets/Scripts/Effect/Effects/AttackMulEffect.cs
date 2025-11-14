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
            float add = Mathf.Max(0f, magnitude);
            mul = 1f + add;
            return true;
        }
        mul = 1f;
        return false;
    }
}

// 사용 예시:
// float finalAtk = baseAtk * AttackMulEffect.GetAttackMultiplier(ownerGameObject);