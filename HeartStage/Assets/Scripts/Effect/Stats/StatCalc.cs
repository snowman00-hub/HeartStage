using UnityEngine;

public static class StatCalc
{
    /// baseValue 에
    /// 1) IStatAddSource 들의 add를 모두 더하고
    /// 2) IStatMulSource 들의 mul을 모두 곱해서
    /// (base + add) * mul 형태로 최종 스탯을 계산.
    public static float GetFinalStat(GameObject owner, StatType stat, float baseValue)
    {
        if (owner == null)
            return baseValue;

        float add = 0f;

        // 1) 덧셈 버프 모으기
        var addSources = owner.GetComponents<IStatAddSource>();
        if (addSources != null)
        {
            for (int i = 0; i < addSources.Length; i++)
            {
                var src = addSources[i];
                if (src == null)
                    continue;

                if (src.TryGetAdd(stat, out var a))
                    add += a;
            }
        }

        // 2) 곱셈 버프 모으기 (기존 StatMultiplier 사용)
        float mul = StatMultiplier.GetTotalMultiplier(owner, stat);

        // 3) 기본값 + 덧셈 → 배율 곱하기
        float result = (baseValue + add) * mul;

        // 4) 확률형 스탯은 0~1 사이로 클램프 (원하면 여기서 관리)
        if (stat == StatType.CritChance ||
            stat == StatType.ExtraAttackChance)
        {
            result = Mathf.Clamp01(result);
        }

        return result;
    }
}
