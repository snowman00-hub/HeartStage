public interface IStatAddSource
{
    /// <summary>
    /// 이 오브젝트에 붙은 "덧셈 버프"가
    /// 해당 StatType에 대해 얼마나 더해줄지 반환.
    /// ex) +1.5 = 사거리 +1.5칸
    /// </summary>
    bool TryGetAdd(StatType stat, out float add);
}

public interface IStatMulSource
{
    /// <summary>
    /// 이 오브젝트에 붙은 "곱셈 버프"가
    /// 해당 StatType에 대해 몇 배를 곱해줄지 반환.
    /// ex) 1.15f = +15%, 0.7f = -30% 느낌
    /// </summary>
    bool TryGetMul(StatType stat, out float mul);
}

public interface IConditionSource
{
    /// <summary>
    /// 특정 ConditionType에 대한 값을 리턴.
    /// - Stun / Paralyze : 0보다 크면 걸려있다고 보는 식 (강도 1f 고정도 가능)
    /// - Confuse        : 0~1 사이의 확률 값
    /// - Knockback      : 거리(또는 강도) 등으로 써도 됨
    /// </summary>
    bool TryGetCondition(ConditionType type, out float value);
}