using UnityEngine;

public class AttackMulEffect : EffectBase
{
    // 추가 컴포넌트 X. OnApply/OnRemove에서 할 일 없음.
    protected override void OnApply() { Debug.Log($"[AttackMulEffect] OnApply mag={magnitude}, dur={duration}", this); }
    protected override void OnRemove() { Debug.Log("[AttackMulEffect] OnRemove", this); }

    // 현재 GameObject에 붙어있는 AttackMulEffect들을 전부 곱해서 반환
    public static float GetAttackMultiplier(GameObject go)
    {
        var effects = go.GetComponents<AttackMulEffect>();
        float mul = 1f;
        for (int i = 0; i < effects.Length; i++)
        {
            // magnitude: 0.15 => ×1.15
            float add = Mathf.Max(0f, effects[i].magnitude);
            mul *= (1f + add);
        }
        return mul;
    }
}

// 사용 예시:
// float finalAtk = baseAtk * AttackMulEffect.GetAttackMultiplier(ownerGameObject);