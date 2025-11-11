using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillData", menuName = "Scriptable Objects/ActiveSkillData")]
public class ActiveSkillData : ScriptableObject
{
    public int skill_id;
    public string skill_name;
    public int use_type;
    public int skill_dmg;
    public float skill_cool;
    public float skill_speed;
    public float skill_crt;
    public float skill_range;
    public int status_id;
    public int buff_id;

    public void UpdateData(ActiveSkillCSVData csvData)
    {
        skill_id = csvData.skill_id;
        skill_name = csvData.skill_name;
        use_type = csvData.use_type;
        skill_dmg = csvData.skill_dmg;
        skill_cool = csvData.skill_cool;
        skill_speed = csvData.skill_speed;
        skill_crt = csvData.skill_crt;
        skill_range = csvData.skill_range;
        status_id = csvData.status_id;
        buff_id = csvData.buff_id;
    }

    public ActiveSkillCSVData ToCSVData()
    {
        ActiveSkillCSVData csvData = new ActiveSkillCSVData();
        csvData.skill_id = skill_id;
        csvData.skill_name = skill_name;
        csvData.use_type = use_type;
        csvData.skill_dmg = skill_dmg;
        csvData.skill_cool = skill_cool;
        csvData.skill_speed = skill_speed;
        csvData.skill_crt = skill_crt;
        csvData.skill_range = skill_range;
        csvData.status_id = status_id;
        csvData.buff_id = buff_id;

        return csvData;
    }
}