public enum Languages
{
    Korean,
}

public static class DataTableIds
{
    public static readonly string[] StringTableIds =
    {
        "StringTableKr",
    };

    public static readonly string[] CsvTableIds =
    {
        "ItemTable",
        "MonsterTable",
    };

    public static string String => StringTableIds[0];

    public static string Item => CsvTableIds[0];

    public static string Monster => CsvTableIds[1];
}

public class Tag
{
    public static readonly string Player = "Player";
    public static readonly string Enemy = "Enemy";
    public static readonly string Wall = "Wall";
}

public class AddressableLabel
{
    public static readonly string Stage = "StageAssets";
}