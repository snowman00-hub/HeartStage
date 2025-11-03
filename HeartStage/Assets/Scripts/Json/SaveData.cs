using System;

[Serializable]
public abstract class SaveData
{
    public int Version { get; protected set; }

    public abstract SaveData VersionUp();
}

[Serializable]
public class SaveDataV1 : SaveData
{
    public int hp = 0;

    public SaveDataV1()
    {
        Version = 1;
        hp = 0;
    }

    public override SaveData VersionUp()
    {
        throw new NotImplementedException();
    }
}