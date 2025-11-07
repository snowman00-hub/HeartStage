public enum Languages
{
    Korean,
}

public class StringIds
{
    public static readonly string Test = "Test";
}

public static class DataTableIds
{
    public static readonly string[] StringTableIds =
    {
        "StringTableKr",
    };

    public static string String => StringTableIds[0];
}

public class Tag
{
    public static readonly string Player = "Player";
    public static readonly string Enemy = "Enemy";
}

public class AddressableLabel
{
    public static readonly string Stage = "StageAssets";
}