public enum Languages
{
    Korean,
}

public static class DataTableIds
{
    public static readonly string[] CsvTableIds =
    {
        "ItemTable",
    };

    public static string Item => CsvTableIds[0];

    public static string Character => "CharacterTable";
    public static string Monster => "MonsterTable";
    public static string ActiveSkill => "ActiveSkillTable";
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