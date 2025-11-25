using UnityEngine;
using UnityEngine.UI;

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
    public static string Reward => "RewardTable";
    public static string Gacha => "GachaTable";
    public static string GachaType => "GachaTypeTable";
    public static string Shop => "ShopTable";
    public static string RankUp => "RankUpTable";
    public static string LevelUp => "LevelUpTable";
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
    Gacha,
    GachaPercentage,
    GachaResult,
    Gacha5TryResult,

    // 인게임 윈도우
    VictoryDefeat = 50, // 위에 추가해도 안바뀌게 큰 값으로 해두기
    CharacterInfo,
}

public class SoundName
{
    public static readonly string SFX_UI_Button_Click = "Ui_click_01";    
}

public class ItemID
{
    public static readonly int LightStick = 7101;
    public static readonly int HeartStick = 7102;
    public static readonly int Exp = 7103;
    public static readonly int DreamEnergy = 7104;
    public static readonly int TrainingPoint = 7105;
}

public class CurrencyIcon
{
    public static readonly string lightStickIcon = "LightstickImage";
    public static readonly string heartStickIcon = "HeartStickImage";
    public static readonly string dollarIcon = "DollarIcon";

    // 0 : 없음
    // 1 : 원화
    // 2 : 달러
    // 7101 : 라이트스틱
    // 7102 : 하트스틱

    public static void CurrencyIconChange(Image image, int id)
    {
        string iconAssetName = id switch
        {
            1 => dollarIcon,
            7101 => lightStickIcon,
            7102 => heartStickIcon,
            _ => null,
        };

        if (iconAssetName == null)
            return;

        var texture = ResourceManager.Instance.Get<Texture2D>(iconAssetName);

        if (texture == null)
            return;

        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
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