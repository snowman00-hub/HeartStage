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
    }
}