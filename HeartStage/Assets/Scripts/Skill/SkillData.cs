using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public int skill_id;
    public string skill_name;
    public int skill_auto;
    public int skill_type;
    public PassiveType passive_type;
    public int skill_target;
    public bool skill_pierce;
    public int skill_dmg;
    public float skill_cool;
    public float skill_speed;
    public float skill_crt;
    public float skill_range;
    public float skill_duration;
    public int summon_min;
    public int summon_max;
    public int summon_type;
    public int skill_eff1;
    public float skill_eff1_val;
    public float skill_eff1_duration;
    public int skill_eff2;
    public float skill_eff2_val;
    public float skill_eff2_duration;
    public int skill_eff3;
    public float skill_eff3_val;
    public float skill_eff3_duration;
    public string info;
    public string icon_prefab;

    public void UpdateData(SkillCSVData csvData)
    {
        skill_id = csvData.skill_id;
        skill_name = csvData.skill_name;
        skill_auto = csvData.skill_auto;
        skill_type = csvData.skill_type;
        passive_type = csvData.passive_type;
        skill_target = csvData.skill_target;
        skill_pierce = csvData.skill_pierce;
        skill_dmg = csvData.skill_dmg;
        skill_cool = csvData.skill_cool;
        skill_speed = csvData.skill_speed;
        skill_crt = csvData.skill_crt;
        skill_range = csvData.skill_range;
        skill_duration = csvData.skill_duration;
        summon_min = csvData.summon_min;
        summon_max = csvData.summon_max;
        summon_type = csvData.summon_type;
        skill_eff1 = csvData.skill_eff1;
        skill_eff1_val = csvData.skill_eff1_val;
        skill_eff1_duration = csvData.skill_eff1_duration;
        skill_eff2 = csvData.skill_eff2;
        skill_eff2_val = csvData.skill_eff2_val;
        skill_eff2_duration = csvData.skill_eff2_duration;
        skill_eff3 = csvData.skill_eff3;
        skill_eff3_val = csvData.skill_eff3_val;
        skill_eff3_duration = csvData.skill_eff3_duration;
        info = csvData.info;
        icon_prefab = csvData.icon_prefab;

    }
    public SkillCSVData ToCSVData()
    {
        SkillCSVData csvData = new SkillCSVData();
        csvData.skill_id = skill_id;
        csvData.skill_name = skill_name;
        csvData.skill_auto = skill_auto;
        csvData.skill_type = skill_type;
        csvData.passive_type = passive_type;
        csvData.skill_target = skill_target;
        csvData.skill_pierce = skill_pierce;
        csvData.skill_dmg = skill_dmg;
        csvData.skill_cool = skill_cool;
        csvData.skill_speed = skill_speed;
        csvData.skill_crt = skill_crt;
        csvData.skill_range = skill_range;
        csvData.skill_duration = skill_duration;
        csvData.summon_min = summon_min;
        csvData.summon_max = summon_max;
        csvData.summon_type = summon_type;
        csvData.skill_eff1 = skill_eff1;
        csvData.skill_eff1_val = skill_eff1_val;
        csvData.skill_eff1_duration = skill_eff1_duration;
        csvData.skill_eff2 = skill_eff2;
        csvData.skill_eff2_val = skill_eff2_val;
        csvData.skill_eff2_duration = skill_eff2_duration;
        csvData.skill_eff3 = skill_eff3;
        csvData.skill_eff3_val = skill_eff3_val;
        csvData.skill_eff3_duration = skill_eff3_duration;
        csvData.info = info;
        csvData.icon_prefab = icon_prefab;

        return csvData;
    }
}

[System.Serializable]
public class SkillCSVData
{
    public int skill_id { get; set; }
    public string skill_name { get; set; }
    public int skill_auto { get; set; }
    public int skill_type { get; set; }
    public PassiveType passive_type { get; set; }
    public int skill_target { get; set; }
    public bool skill_pierce { get; set; }
    public int skill_dmg { get; set; }
    public float skill_cool { get; set; }
    public float skill_speed { get; set; }
    public float skill_crt { get; set; }
    public float skill_range { get; set; }
    public float skill_duration { get; set; }
    public int summon_min { get; set; }
    public int summon_max { get; set; }
    public int summon_type { get; set; }
    public int skill_eff1 { get; set; }
    public float skill_eff1_val { get; set; }
    public float skill_eff1_duration { get; set; }
    public int skill_eff2 { get; set; }
    public float skill_eff2_val { get; set; }
    public float skill_eff2_duration { get; set; }
    public int skill_eff3 { get; set; }
    public float skill_eff3_val { get; set; }
    public float skill_eff3_duration { get; set; }
    public string info { get; set; }
    public string icon_prefab { get; set; }
}

public enum PassiveType
{
    None = 0,
    Type1 = 1,
    Type2 = 2,
    Type3 = 3,
    Type4 = 4,
    Type5 = 5,
    Type6 = 6,
    Type7 = 7,
    Type8 = 8,
}