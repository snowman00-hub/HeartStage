using System.Collections.Generic;
using UnityEngine;

public static class StageLayoutUtil
{
    public const int SlotCount = 15; // 5x3 고정 공간

    // type -> 활성 인덱스 목록
    private static readonly Dictionary<StageType, int[]> EnabledIndices
        = new Dictionary<StageType, int[]>
    {
        // 전체 오픈
        { StageType.Full, new [] {
            0,1,2,3,4,
            5,6,7,8,9,
            10,11,12,13,14
        }},
        // 중앙 3x3 : 1 2 3 / 6 7 8 / 11 12 13
        { StageType.Stage1, new [] {
            1,2,3,      // 1,2,3
            6,7,8,      // 6,7,8
            11,12,13    // 11,12,13
        }},
        //Stage2 : 1 2 3 / 6 7 8 / 10 11 12 13 14
        { StageType.Stage2, new []{
                1,2,3,
                6,7,8,
                10,11,12,13,14
        }},
            
        // 아래는 예시 템플릿 (원하는 모양으로 바꿔 끼우면 됨)
        //{ StageLayoutType.Left_3x3, new [] {
        //    0,1,2,
        //    5,6,7,
        //    10,11,12
        //}},
        //{ StageLayoutType.Right_3x3, new [] {
        //    2,3,4,
        //    7,8,9,
        //    12,13,14
        //}},
    };

    public static bool[] BuildMask(int stageTypeInt)
    {
        var type = (StageType)stageTypeInt;

        bool[] mask = new bool[SlotCount];

        if (!EnabledIndices.TryGetValue(type, out var indices))
        {
            // 정의 안 된 타입이면 안전하게 전체 오픈
            indices = EnabledIndices[StageType.Full];
        }

        foreach (var i in indices)
            if (i >= 0 && i < SlotCount) mask[i] = true;

        return mask;
    }
}

