using UnityEngine;

[CreateAssetMenu(fileName = "SynergyData", menuName = "Scriptable Objects/SynergyData")]
public class SynergyData : ScriptableObject
{
   public int synergy_id;
   public string synergy_name;
   public int synergy_Unit1;
   public int synergy_Unit2;
   public int synergy_Unit3;
   public int skill_target;
   public int effect_type1;
   public float effect_val1;
   public int effect_type2;
   public float effect_val2;
   public int effect_type3;
   public float effect_val3;
   public string synergy_required;
   public string synergy_info;

    public SynergyCSVData UpdateData(SynergyCSVData csvData)
    {
        synergy_id = csvData.synergy_id;
        synergy_name = csvData.synergy_name;
        synergy_Unit1 = csvData.synergy_Unit1;
        synergy_Unit2 = csvData.synergy_Unit2;
        synergy_Unit3 = csvData.synergy_Unit3;
        skill_target = csvData.skill_target;
        effect_type1 = csvData.effect_type1;
        effect_val1 = csvData.effect_val1;
        effect_type2 = csvData.effect_type2;
        effect_val2 = csvData.effect_val2;
        effect_type3 = csvData.effect_type3;
        effect_val3 = csvData.effect_val3;
        synergy_required = csvData.synergy_required;
        synergy_info = csvData.synergy_info;

        return csvData;
    }

    public SynergyCSVData ToCSVData()
    {
        SynergyCSVData csvData = new SynergyCSVData
        {
            synergy_id = synergy_id,
            synergy_name = synergy_name,
            synergy_Unit1 = synergy_Unit1,
            synergy_Unit2 = synergy_Unit2,
            synergy_Unit3 = synergy_Unit3,
            skill_target = skill_target,
            effect_type1 = effect_type1,
            effect_val1 = effect_val1,
            effect_type2 = effect_type2,
            effect_val2 = effect_val2,
            effect_type3 = effect_type3,
            effect_val3 = effect_val3,
            synergy_required = synergy_required,
            synergy_info = synergy_info
        };
        return csvData;
    }
}


[System.Serializable]
public class SynergyCSVData
{
    public int synergy_id { get; set; }
    public string synergy_name { get; set; }
    public int synergy_Unit1 { get; set; }
    public int synergy_Unit2 { get; set; }
    public int synergy_Unit3 { get; set; }
    public int skill_target { get; set; }
    public int effect_type1 { get; set; }
    public float effect_val1 { get; set; }
    public int effect_type2 { get; set; }
    public float effect_val2 { get; set; }
    public int effect_type3 { get; set; }
    public float effect_val3 { get; set; }
    public string synergy_required { get; set; }
    public string synergy_info { get; set; }
} 