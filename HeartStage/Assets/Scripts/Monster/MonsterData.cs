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

    public void Init(int monsterId) 
    {
        var monsterTable = DataTableManager.MonsterTable;

        if (monsterTable == null)
            return;

        var data = monsterTable.Get(monsterId);
        if(data == null)
        {
            Debug.Log("몬스터 Id가 없습니다.");
            return;
        }

        id = data.id;
        hp = data.hp;
        att = data.atk_dmg;
        attType = data.atk_type;
        attackSpeed = data.atk_speed;
        attackRange = data.atk_range;
        bulletSpeed = data.bullet_speed;
        moveSpeed = data.speed;
        monsterName = data.mon_name;
        monsterType = data.mon_type;
        minExp = data.min_level;
        maxExp = data.max_level;
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
            min_level = minExp,
            max_level = maxExp,
            stage_num = 1, // 기본값 (필요시 별도 필드 추가)
            skill_id = 0   // 기본값 (필요시 별도 필드 추가)
        };
    }
}