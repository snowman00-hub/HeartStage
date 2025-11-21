using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    public int stage_ID;
    public string stage_name;
    public int stage_step1;
    public int stage_step2;
    public int stage_type;
    public int member_count;
    public int dispatch_member;
    public int debut_stamina;
    public int regular_stamina;
    public int wave_time;
    public int wave1_id;
    public int wave2_id;
    public int wave3_id;
    public int wave4_id;
    public int fail_stamina;
    public string prefab;

    public void UpdateData(StageCSVData csvData)
    {
        stage_ID = csvData.stage_ID;
        stage_name = csvData.stage_name;
        stage_step1 = csvData.stage_step1;
        stage_step2 = csvData.stage_step2;
        stage_type = csvData.stage_type;
        member_count = csvData.member_count;
        dispatch_member = csvData.dispatch_member;
        debut_stamina = csvData.debut_stamina;
        regular_stamina = csvData.regular_stamina;
        wave_time = csvData.wave_time;
        wave1_id = csvData.wave1_id;
        wave2_id = csvData.wave2_id;
        wave3_id = csvData.wave3_id;
        wave4_id = csvData.wave4_id;
        fail_stamina = csvData.fail_stamina;
        prefab = csvData.prefab;
    }

    public StageCSVData ToCSVData()
    {
        StageCSVData csvData = new StageCSVData();
        csvData.stage_ID = stage_ID;
        csvData.stage_name = stage_name;
        csvData.stage_step1 = stage_step1;
        csvData.stage_step2 = stage_step2;
        csvData.stage_type = stage_type;
        csvData.member_count = member_count;
        csvData.dispatch_member = dispatch_member;
        csvData.debut_stamina = debut_stamina;
        csvData.regular_stamina = regular_stamina;
        csvData.wave_time = wave_time;
        csvData.wave1_id = wave1_id;
        csvData.wave2_id = wave2_id;
        csvData.wave3_id = wave3_id;
        csvData.wave4_id = wave4_id;
        csvData.fail_stamina = fail_stamina;
        csvData.prefab = prefab;
        return csvData;
    }
}

public enum StageType
{
    Full = 0,      // 15칸 전체
    Stage1 = 1,    // Stage1 1,2,3 / 6,7,8 / 11,12,13
    Stage2 = 2,    // Stage2
}