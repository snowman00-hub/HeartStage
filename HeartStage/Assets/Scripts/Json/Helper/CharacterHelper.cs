using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterHelper
{
    private static List<int> OwnCharacterIds => SaveLoadManager.Data.ownedIds;
    private static Dictionary<int, int> CharacterExpById => SaveLoadManager.Data.expById;
    private static Dictionary<string, bool> UnlockedByName => SaveLoadManager.Data.unlockedByName;

    //캐릭터 획득 처리
    public static void AcquireCharacter(int baseId, CharacterTable charTable)
    {
        var row = charTable.Get(baseId);
        if (row == null)
            return;

        string name = row.char_name;

        // 1) 도감/해금 true
        UnlockedByName[name] = true;

        // 2) 보유 id 등록
        OwnCharacterIds.Add(baseId);

        // 3) exp 초기화
        if (!CharacterExpById.ContainsKey(baseId))
            CharacterExpById[baseId] = 0;

        SaveLoadManager.SaveToServer().Forget(); // 원하면 즉시 저장
    }

    public static void ReplaceOwnedId(int currentId, int nextId, int remainExp)
    {
        //레벨 업 후 or 랭크 업 후 호출
        // id 교체 및 경험치 갱신
        int idx = OwnCharacterIds.IndexOf(currentId);
        if (idx < 0)
            return;

        OwnCharacterIds[idx] = nextId;

        CharacterExpById.Remove(currentId);
        CharacterExpById[nextId] = remainExp;
    }
    public static void CommitUpgradeResult(int startId, int finalId, int remainExp = 0)
    {
        //레벨 업/랭크 업 결과 확정 처리
        if (finalId != startId)
        {
            ReplaceOwnedId(startId, finalId, remainExp);
        }
        else
        {
            // 레벨업/랭크업 안 됐으면 exp만 업데이트
            CharacterExpById[startId] = remainExp;
        }

        SaveLoadManager.SaveToServer().Forget(); // 최종 1회 저장
    }
}
