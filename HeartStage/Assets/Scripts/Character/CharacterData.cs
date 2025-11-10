using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int ID;
    public string ch_name;
    public int ch_level;
    public int ch_level_count;
    public int rank;
    public int atk_name;
    public int atk_info;
    public int atk_effect;
    public int atk_dmg;
    public float atk_speed;
    public float atk_range;
    public float bullet_speed;
    public int bullet_count;
    public int Stamina;
    public float crt_chance;
    public float crt_dmg;
    public int passive_id;
    public int skill_id;
    public int synergy_id;
    public int Unlock;
    public string Info;
    public string bullet_PrefabName;
    public string data_AssetName;

    public void UpdateData(CharacterCSVData csvData)
    {
        ID = csvData.ID;
        ch_name = csvData.ch_name;
        ch_level = csvData.ch_level;
        ch_level_count = csvData.ch_level_count;
        rank = csvData.rank;
        atk_name = csvData.atk_name;
        atk_info = csvData.atk_info;
        atk_effect = csvData.atk_effect;
        atk_dmg = csvData.atk_dmg;
        atk_speed = csvData.atk_speed;
        atk_range = csvData.atk_range;
        bullet_speed = csvData.bullet_speed;
        bullet_count = csvData.bullet_count;
        Stamina = csvData.Stamina;
        crt_chance = csvData.crt_chance;
        crt_dmg = csvData.crt_dmg;
        passive_id = csvData.passive_id;
        skill_id = csvData.skill_id;
        synergy_id = csvData.synergy_id;
        Unlock = csvData.Unlock;
        Info = csvData.Info;
        bullet_PrefabName = csvData.bullet_PrefabName;
        data_AssetName = csvData.data_AssetName;
    }

    public CharacterCSVData ToCSVData()
    {
        CharacterCSVData csvData = new CharacterCSVData();
        csvData.ID = ID;
        csvData.ch_name = ch_name;
        csvData.ch_level = ch_level;
        csvData.ch_level_count = ch_level_count;
        csvData.rank = rank;
        csvData.atk_name = atk_name;
        csvData.atk_info = atk_info;
        csvData.atk_effect = atk_effect;
        csvData.atk_dmg = atk_dmg;
        csvData.atk_speed = atk_speed;
        csvData.atk_range = atk_range;
        csvData.bullet_speed = bullet_speed;
        csvData.bullet_count = bullet_count;
        csvData.Stamina = Stamina;
        csvData.crt_chance = crt_chance;
        csvData.crt_dmg = crt_dmg;
        csvData.passive_id = passive_id;
        csvData.skill_id = skill_id;
        csvData.synergy_id = synergy_id;
        csvData.Unlock = Unlock;
        csvData.Info = Info;
        csvData.bullet_PrefabName = bullet_PrefabName;
        csvData.data_AssetName = data_AssetName;

        return csvData;
    }
}