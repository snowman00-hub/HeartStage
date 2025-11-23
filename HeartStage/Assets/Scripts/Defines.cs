using UnityEngine;

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
    public static string Skill => "SkillTable";
    public static string Effect => "EffectTable";
    public static string Stage => "StageTable";
    public static string Select => "SelectTable";
    public static string Synergy => "SynergyTable";
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
    public static readonly string Tower = "Tower";
}

public class AddressableLabel
{
    public static readonly string Stage = "StageAssets";
}

public enum WindowType
{
    None = -1,
    // 로비 윈도우
    LobbyHome = 0,
    StageSelect,
    StageInfo,
    // 인게임 윈도우
    VictoryDefeat,
    CharacterInfo,
}

public class ItemID
{
    public static readonly int Exp = 7103;
}

public static class StatPower
{
    // 능력치 파워 = (능력치 / 기준값(BaseLine)) * 영향도(Weight)
    private static float vocalBaseLine = 1f;
    private static float vocalWeight = 0.3f;

    private static float labBaseLine = 0.01f;
    private static float labWeight = 0.2f;

    private static float charismaBaseLine = 0.05f;
    private static float charismaWeight = 0.1f;

    private static float cutyBaseLine = 0.05f;
    private static float cutyWeight = 0.05f;

    private static float danceBaseLine = 10f;
    private static float danceWeight = 0.15f;

    private static float visualBaseLine = 0.05f;
    private static float visualWeight = 0.1f;

    private static float sexyBaseLine = 0.01f;
    private static float sexyWeight = 0.1f;


    // --------------------------
    //        Power Functions
    // --------------------------

    public static int GetVocalPower(float value)
    {
        return Mathf.CeilToInt((value / vocalBaseLine) * vocalWeight);
    }

    public static int GetLabPower(float value)
    {
        return Mathf.CeilToInt((value / labBaseLine) * labWeight);
    }

    public static int GetCharismaPower(float value)
    {
        return Mathf.CeilToInt((value / charismaBaseLine) * charismaWeight);
    }

    public static int GetCutyPower(float value)
    {
        return Mathf.CeilToInt((value / cutyBaseLine) * cutyWeight);
    }

    public static int GetDancePower(float value)
    {
        return Mathf.CeilToInt((value / danceBaseLine) * danceWeight);
    }

    public static int GetVisualPower(float value)
    {
        return Mathf.CeilToInt((value / visualBaseLine) * visualWeight);
    }

    public static int GetSexyPower(float value)
    {
        return Mathf.CeilToInt((value / sexyBaseLine) * sexyWeight);
    }
}