using System;
using System.Collections.Generic;

[Serializable]
public abstract class SaveData
{
    public int Version { get; protected set; }

    public abstract SaveData VersionUp();
}

[Serializable]
public class SaveDataV1 : SaveData
{
    public Dictionary<int, int> itemList = new Dictionary<int, int>(); // 아이템 ID와 수량을 저장
    public List<int> clearWaveList = new List<int>(); // 클리어한 웨이브 ID를 저장 -> 최초 보상 체크
    public List<int> ownedIds = new List<int>(); // 보유 캐릭터 아이디 집합 (중복 방지)
    public Dictionary<string, bool> unlockedByName = new Dictionary<string, bool>(); // 해금 여부를 Name별로 저장
    public Dictionary<int, int> expById = new Dictionary<int, int>(); // 경험치를 ID별로 저장

    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        throw new NotImplementedException();
    }
}