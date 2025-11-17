using UnityEngine;

public static class StatMultiplier
{
    /// <summary>
    /// owner에 붙어 있는 모든 IStatMulSource를 모아서
    /// 지정한 StatType에 대한 총 배율(곱셈)을 반환.
    /// 아무 소스가 없으면 1f 반환.
    /// </summary>
    public static float GetTotalMultiplier(GameObject owner, StatType stat)
    {
        float mul = 1f;

        if (owner == null)
            return mul;

        var sources = owner.GetComponents<IStatMulSource>();
        if (sources == null || sources.Length == 0)
            return mul;

        for (int i = 0; i < sources.Length; i++)
        {
            var src = sources[i];
            if (src == null)
                continue;

            if (src.TryGetMul(stat, out float m))
            {
                // 각 Effect에서 "최종 배율"을 넘겨준다고 가정.
                // ex) AttackMulEffect 에서 1f + magnitude 로 계산 후 반환.
                mul *= m;
            }
        }

        return mul;
    }

    /// <summary>
    /// 편의용 확장 메서드 (go.GetStatMul(StatType.Attack) 이런 식으로 사용)
    /// </summary>
    public static float GetStatMul(this GameObject owner, StatType stat)
    {
        return GetTotalMultiplier(owner, stat);
    }
}
