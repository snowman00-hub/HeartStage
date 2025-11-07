using UnityEngine;

public class MonsterDataController : MonoBehaviour
{
    [Header("Field")]
    private MonsterData data;
    public int hp;
    public int att;
    public float moveSpeed;

    public void Init(MonsterData monsterData)
    {
        data = monsterData;
        hp = monsterData.hp;
        att = monsterData.att;
        moveSpeed = monsterData.moveSpeed;
    }
}
