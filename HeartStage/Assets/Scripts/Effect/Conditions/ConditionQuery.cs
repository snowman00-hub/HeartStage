using UnityEngine;

public static class ConditionQuery
{
    /// 단순히 "이 Condition이 걸려있냐?"만 보고 싶을 때
    public static bool HasCondition(GameObject owner, ConditionType type)
    {
        return GetConditionValue(owner, type) > 0f;
    }

    /// Confuse 확률처럼 값이 의미 있을 때
    public static float GetConditionValue(GameObject owner, ConditionType type)
    {
        if (owner == null)
            return 0f;

        float maxValue = 0f;

        var sources = owner.GetComponents<IConditionSource>();
        if (sources == null || sources.Length == 0)
            return 0f;

        for (int i = 0; i < sources.Length; i++)
        {
            var src = sources[i];
            if (src == null)
                continue;

            if (src.TryGetCondition(type, out var v))
            {
                // 일단 가장 강한 값만 사용 (필요하면 Sum 등으로 바꿀 수 있음)
                if (v > maxValue)
                    maxValue = v;
            }
        }

        return maxValue;
    }
}
