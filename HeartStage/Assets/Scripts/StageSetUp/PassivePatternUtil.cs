using System.Collections.Generic;
using UnityEngine;

public static class PassivePatternUtil
{
    // 5x3 그리드 기준 (0~14)
    private const int Columns = 5;
    private const int Rows = 3;

    // 🔹 PassiveType별 패턴 정의 (행/열 오프셋)
    //   중심칸은 항상 (0, 0)
    private static readonly Dictionary<PassiveType, Vector2Int[]> Patterns =
     new Dictionary<PassiveType, Vector2Int[]>
 {
    // 1: 자기 + 자기 아래 (7, 12)
    { PassiveType.Type1, new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        }
    },

    // 2: 자기 위 + 자기 + 자기 아래 (2, 7, 12)
    { PassiveType.Type2, new[]
        {
            new Vector2Int(-1, 0),
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        }
    },

    // 3: 자기 + 자기 우 아래 (7, 13)
    { PassiveType.Type3, new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
        }
    },

    // 4: 자기 왼 위 + 자기 + 자기 우 아래 (1, 7, 13)
    { PassiveType.Type4, new[]
        {
            new Vector2Int(-1, -1),
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
        }
    },

    // 5: 자기 + 자기 왼 아래 (7, 11)
    { PassiveType.Type5, new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, -1),
        }
    },

    // 6: 자기 우 위 + 자기 + 자기 왼 아래 (3, 7, 11)
    { PassiveType.Type6, new[]
        {
            new Vector2Int(-1,  1),
            new Vector2Int(0,   0),
            new Vector2Int(1,  -1),
        }
    },

    // 7: 자기 + 자기 우 (7, 8)
    { PassiveType.Type7, new[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
        }
    },

    // 8: 자기 왼 + 자기 + 자기 우 (6, 7, 8)
    { PassiveType.Type8, new[]
        {
            new Vector2Int(0, -1),
            new Vector2Int(0,  0),
            new Vector2Int(0,  1),
        }
    },
 };

    /// <summary>
    /// 중심 index와 PassiveType을 기준으로, 실제로 색칠/버프가 들어가는 타일 인덱스들 리턴
    /// </summary>
    public static IEnumerable<int> GetPatternTiles(int centerIndex, PassiveType type, int slotCount)
    {
        if (!Patterns.TryGetValue(type, out var offsets))
            yield break;

        int total = slotCount;
        int centerRow = centerIndex / Columns;
        int centerCol = centerIndex % Columns;

        foreach (var offset in offsets)
        {
            int r = centerRow + offset.x;
            int c = centerCol + offset.y;

            if (r < 0 || r >= Rows || c < 0 || c >= Columns)
                continue;

            int idx = r * Columns + c;
            if (idx < 0 || idx >= total)
                continue;

            yield return idx;
        }
    }
}
