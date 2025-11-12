// Patched (no functional changes) - kept enums and struct as-is for Mul-only path.
﻿using System.Collections.Generic;

public enum StatType
{
    None = 0,
    Attack = 3001, // 공격 증감
    AttackSpeed = 3002, // 공속 증감
    AttackRange = 3003, // 사거리 증감
    ExtraAttackChance = 3004, // 추가 공격 확률
    HP = 3005, // 체력 증감
    CritChance = 3006, // 치확 증감
    CritDamage = 3007, // 치피 증감
    ShoutGain = 3008, // 함성 증감(게이지)
    DropGain = 3009, // 재화 증감
    Speed = 3010, // 이속 증감
    IncreasedDamageTaken = 3016, // 피해증가(받는 피해 증감)
    IncreasedProjectile = 3017, // 투사체 증가(1회 추가 투사체)
}
public enum AbnormalStat
{
    None = 0,
    Sturn = 3011, // 이동, 공격 모두 불가능
    Paralysis = 3012, // 이동불가, 공격 가능
    Disarray = 3013, // 확률적으로 아군 공격
    Knockback = 3014, // 뒤로 수치만큼 이동
    Spwan = 3015, // 특정 객체 소환  (네이밍 고정)
}
public static class EffectDisplayKR
{
    public static readonly Dictionary<StatType, (string name, string info)> StatText =
        new()
        {
            { StatType.Attack,               ("공격 증감", "공격력 증가") },
            { StatType.AttackSpeed,          ("공속 증감", "공격속도 증가") },
            { StatType.AttackRange,          ("사거리 증감", "공격 사거리 증가") },
            { StatType.ExtraAttackChance,    ("추가 공격 확률", "추가 공격 확률 증가") },
            { StatType.HP,                   ("체력 증감", "체력 증가") },
            { StatType.CritChance,           ("치확 증감", "치명타 확률 증가") },
            { StatType.CritDamage,           ("치피 증감", "치명타 피해 증가") },
            { StatType.ShoutGain,            ("함성 증감", "함성 게이지 획득량 증가") },
            { StatType.DropGain,             ("재화 증감", "드랍 아이템 획득량 증가") },
            { StatType.Speed,                ("이속 증감", "이동속도 증가") },
            { StatType.IncreasedDamageTaken, ("피해증가", "받는 피해 증감") },
            { StatType.IncreasedProjectile,  ("투사체 증가", "한번 공격에 투사체를 하나더 던진다") },
        };

    public static readonly Dictionary<AbnormalStat, (string name, string info)> StatusText =
        new()
        {
            { AbnormalStat.Sturn,     ("스턴",   "이동, 공격 모두 불가능") },
            { AbnormalStat.Paralysis, ("마비",   "이동불가, 공격 가능") },
            { AbnormalStat.Disarray,  ("혼란",   "확률적으로 아군 공격") },
            { AbnormalStat.Knockback, ("넉백",   "뒤로 수치만큼 이동") },
            { AbnormalStat.Spwan,     ("소환",   "특정 객체 소환") },
        };
}

public enum ModOp
{ 
    Addfloat,
    AddPercent,
    MulPercent,
}

public struct StatModifier
{
    public StatType statType;
    public ModOp modOp;
    public float value;
    public string source;
    public int id;
}