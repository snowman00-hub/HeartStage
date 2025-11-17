using UnityEngine;

public static class StatCalc
{
    /// baseValue 에
    /// 1) IStatAddSource 들의 add를 모두 더하고
    /// 2) IStatMulSource 들의 mul을 모두 곱해서
    /// (base + add) * mul 형태로 합친 뒤,
    /// StatType 성격에 맞게 후처리까지 해서 반환.
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
                if (src == null) continue;

                if (src.TryGetAdd(stat, out var a))
                    add += a;
            }
        }

        // 2) 곱셈 버프 모으기
        float mul = StatMultiplier.GetTotalMultiplier(owner, stat);

        // 3) 기본 합성
        float raw = (baseValue + add) * mul;

        // 4) StatType 성격에 맞게 후처리
        switch (stat)
        {
            // 일반 실수 스탯 (0 이상이면 됨)
            case StatType.Attack:
            case StatType.AttackSpeed:
            case StatType.AttackRange:
            case StatType.MaxHp:
            case StatType.MoveSpeed:
            case StatType.CritDamage:
            case StatType.ShoutGainRate:
            case StatType.DropAmountRate:
            case StatType.IncomingDamage:
            case StatType.CritChance:
            case StatType.ExtraAttackChance:
                return Mathf.Max(0f, raw);

            // 개수(정수)
            case StatType.ProjectileCount:
                return Mathf.Max(0, Mathf.RoundToInt(raw));

            default:
                return raw;
        }
    }
}
