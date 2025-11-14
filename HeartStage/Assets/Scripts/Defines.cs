using System.Collections.Generic;

public enum Languages
{
    Korean,
}

public static class DataTableIds
{
    public static string Item => "ItemTable";
    public static string Character => "CharacterTable";
    public static string Monster => "MonsterTable";
    public static string StageWave => "StageWaveTable";
    public static string ActiveSkill => "ActiveSkillTable";
    public static string Skill => "SkillTable";
    public static string Effect => "EffectTable";

    public static string Stage => "StageTable";

}

public static class IBuffIds
{
    public static readonly string[] BuffIds =
    {
        "AttackPowerBuff",
    };
    public static string AttackPowerBuff => BuffIds[0];
}

public class Tag
{
    public static readonly string Player = "Player";
    public static readonly string Monster = "Monster";
    public static readonly string Wall = "Wall";
}

public class AddressableLabel
{
    public static readonly string Stage = "StageAssets";
}

public enum WindowType
{
    None = -1,
    TestWindow,
    Test2Window,
    // 메인 UI 등 추가
}