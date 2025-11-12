using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int id;
    public string monsterName;
    public int monsterType;
    public int hp;
    public int att;
    public int attType;
    public int attackSpeed;
    public int attackRange;
    public int bulletSpeed;
    public int moveSpeed;
    public int minExp;
    public int maxExp;
    public string image_AssetName;

    // CharacterData처럼 UpdateData 구현
    public void UpdateData(MonsterCSVData csvData)
    {
        id = csvData.id;
        hp = csvData.hp;
        att = csvData.atk_dmg;
        attType = csvData.atk_type;
        attackSpeed = csvData.atk_speed;
        attackRange = csvData.atk_range;
        bulletSpeed = csvData.bullet_speed;
        moveSpeed = csvData.speed;
        monsterName = csvData.mon_name;
        monsterType = csvData.mon_type;
        minExp = csvData.min_level;
        maxExp = csvData.max_level;
        image_AssetName = csvData.image_AssetName;
    }

    // 기존 Init 메서드는 유지 (하위 호환성)
    public void Init(int monsterId)
    {
        var monsterTable = DataTableManager.MonsterTable;

        if (monsterTable == null)
            return;

        var data = monsterTable.Get(monsterId);
        if (data == null)
        {
            Debug.Log("몬스터 Id가 없습니다.");
            return;
        }

        UpdateData(data); // UpdateData 사용하도록 변경
    }

    public MonsterCSVData ToTableData()
    {
        return new MonsterCSVData
        {
            id = id,
            mon_name = monsterName,
            mon_type = monsterType,
            hp = hp,
            atk_dmg = att,
            atk_type = attType,
            atk_speed = attackSpeed,
            atk_range = attackRange,
            bullet_speed = bulletSpeed,
            speed = moveSpeed,
            image_AssetName = image_AssetName,
            min_level = minExp,
            max_level = maxExp,
            stage_num = 1,
            skill_id = 0
        };
    }
}