using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int ID;
    public float atk_dmg;
    public float atk_interval;
    public float atk_range;
    public float bullet_speed;
    public float bullet_count;
    public float hp;
    public float crt_chance;
    public float crt_hit_rate;
    public string bullet_PrefabName;
    public string data_AssetName;

    public void UpdateData(CharacterCSVData csvData)
    {
        ID = csvData.ID;
        atk_dmg = csvData.atk_dmg;
        atk_interval = csvData.atk_interval;
        atk_range = csvData.atk_range;
        bullet_speed = csvData.bullet_speed;
        bullet_count = csvData.bullet_count;
        hp = csvData.hp;
        crt_chance = csvData.crt_chance;
        crt_hit_rate = csvData.crt_hit_rate;
        bullet_PrefabName = csvData.bullet_PrefabName;
        data_AssetName = csvData.data_AssetName;
    }

    public CharacterCSVData ToCSVData()
    {
        CharacterCSVData csvData = new CharacterCSVData();
        csvData.ID = ID;
        csvData.atk_dmg = atk_dmg;
        csvData.atk_interval = atk_interval;
        csvData.atk_range = atk_range;
        csvData.bullet_speed = bullet_speed;
        csvData.bullet_count = bullet_count;
        csvData.hp = hp;
        csvData.crt_chance = crt_chance;
        csvData.crt_hit_rate= crt_hit_rate;
        csvData.bullet_PrefabName = bullet_PrefabName;
        csvData.data_AssetName = data_AssetName;

        return csvData;
    }
}