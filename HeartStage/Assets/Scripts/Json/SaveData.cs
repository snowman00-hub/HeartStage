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

    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        throw new NotImplementedException();
    }
}