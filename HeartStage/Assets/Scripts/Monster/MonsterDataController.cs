using UnityEngine;

public class MonsterDataController : MonoBehaviour
{
    [Header("Field")]
    private MonsterData data;
    public int hp;

    public void Init(MonsterData monsterData)
    {
        data = monsterData;
        hp = monsterData.hp;        
    }
}
