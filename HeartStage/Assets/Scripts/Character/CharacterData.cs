using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int char_id;
    public string char_name;
    public int char_lv;
    public int char_exp;
    public int char_rank;
    public int char_type;

    public int atk_dmg;
    public float atk_speed;
    public float atk_range;
    public float atk_addcount;

    public int bullet_count;
    public float bullet_speed;
    public int char_hp;

    public float crt_chance;
    public float crt_dmg;

    public int skill_id1;
    public int skill_id2;
    public int skill_id3;
    public int skill_id4;
    public int skill_id5;
    public int skill_id6;

    public string Info;

    public string image_PrefabName;
    public string data_AssetName;
    public string bullet_PrefabName;
    public string projectile_AssetName;
    public string hitEffect_AssetName;

    // 전투력 주기
    public int GetTotalPower()
    {
        float total = atk_dmg + atk_speed + char_hp + crt_chance + crt_dmg + atk_addcount + atk_range;
        return Mathf.FloorToInt(total);
    }

    // CSV → ScriptableObject
    public void UpdateData(CharacterCSVData csv)
    {
        char_id = csv.char_id;
        char_name = csv.char_name;
        char_lv = csv.char_lv;
        char_exp = csv.char_exp;
        char_rank = csv.char_rank;
        char_type = csv.char_type;

        atk_dmg = csv.atk_dmg;
        atk_speed = csv.atk_speed;
        atk_range = csv.atk_range;
        atk_addcount = csv.atk_addcount;

        bullet_count = csv.bullet_count;
        bullet_speed = csv.bullet_speed;
        char_hp = csv.char_hp;

        crt_chance = csv.crt_chance;
        crt_dmg = csv.crt_dmg;

        skill_id1 = csv.skill_id1;
        skill_id2 = csv.skill_id2;
        skill_id3 = csv.skill_id3;
        skill_id4 = csv.skill_id4;
        skill_id5 = csv.skill_id5;
        skill_id6 = csv.skill_id6;

        Info = csv.Info;

        image_PrefabName = csv.image_PrefabName;
        data_AssetName = csv.data_AssetName;
        bullet_PrefabName = csv.bullet_PrefabName;
        projectile_AssetName = csv.projectile_AssetName;
        hitEffect_AssetName = csv.hitEffect_AssetName;
    }

    // ScriptableObject → CSV
    public CharacterCSVData ToCSVData()
    {
        return new CharacterCSVData
        {
            char_id = char_id,
            char_name = char_name,
            char_lv = char_lv,
            char_exp = char_exp,
            char_rank = char_rank,
            char_type = char_type,

            atk_dmg = atk_dmg,
            atk_speed = atk_speed,
            atk_range = atk_range,
            atk_addcount = atk_addcount,

            bullet_count = bullet_count,
            bullet_speed = bullet_speed,
            char_hp = char_hp,

            crt_chance = crt_chance,
            crt_dmg = crt_dmg,

            skill_id1 = skill_id1,
            skill_id2 = skill_id2,
            skill_id3 = skill_id3,
            skill_id4 = skill_id4,
            skill_id5 = skill_id5,
            skill_id6 = skill_id6,

            Info = Info,

            image_PrefabName = image_PrefabName,
            data_AssetName = data_AssetName,
            bullet_PrefabName = bullet_PrefabName,
            projectile_AssetName = projectile_AssetName,
            hitEffect_AssetName = hitEffect_AssetName
        };
    }
}

public enum CharacterType
{
    None = 0, // 없음
    Vocal = 1, // 보컬 - 공격력
    Rap = 2, // 랩 - 공격속도
    Charisma = 3, // 카리스마 - 사거리
    Cutie = 4, // 큐티 - 추가 공격 확률
    Dance = 5, // 댄스 - 체력
    Visual = 6, // 비주얼 - 치명타 확률
    Sexy = 7, // 섹시 - 치명타 피해
}