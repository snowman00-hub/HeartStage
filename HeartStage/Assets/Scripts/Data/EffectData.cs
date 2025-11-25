using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "Scriptable Objects/EffectData")]
public class EffectData : ScriptableObject
{
    public int effect_ID;
    public string effect_name;
    public string effect_info;

    public void UpdateData(EffectCSVData csvData)
    {
        effect_ID = csvData.effect_ID;
        effect_name = csvData.effect_name;
        effect_info = csvData.effect_info;
    }
    public EffectCSVData ToCSVData()
    {
        EffectCSVData csvData = new EffectCSVData();
        csvData.effect_ID = effect_ID;
        csvData.effect_name = effect_name;
        csvData.effect_info = effect_info;
        return csvData;
    }
}

[System.Serializable]
public class EffectCSVData
{
    public int effect_ID { get; set; }
    public string effect_name { get; set; }
    public string effect_info { get; set; }
}

