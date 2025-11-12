using UnityEngine;
using Cysharp.Threading.Tasks;

public class DeceptionBossSkill : MonoBehaviour, ISkillBehavior
{
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private string poolId = "121042";

    private void Awake()
    {
        if (string.IsNullOrEmpty(poolId))
            poolId = "121042";

        if (spawnCount <= 0)
            spawnCount = 5;
    }

    public void Execute()
    {
        var monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior != null)
        {
            DeceptionSkill(monsterBehavior).Forget();
        }
    }

    public async UniTaskVoid DeceptionSkill(MonsterBehavior boss)
    {
        Debug.Log($"대량 현혹 스킬 실행: poolId={poolId}, spawnCount={spawnCount}");

        for (int i = 0; i < spawnCount; i++)
        {
            int spawnPosX = Random.Range(0, Screen.width);           

            Vector3 screenPosition = new Vector3(spawnPosX, Screen.height, 0);
            Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
            spawnPos.z = 0f;

            var monster = PoolManager.Instance.Get(poolId);
            if (monster != null)
            {
                monster.transform.position = spawnPos;
                monster.transform.rotation = Quaternion.identity;
                monster.SetActive(true);

                var monsterData = ScriptableObject.CreateInstance<MonsterData>();
                monsterData.Init(111011); // test

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                if (monsterBehavior != null)
                {
                    monsterBehavior.Init(monsterData);
                }

                MonsterSpawner.SetMonsterSprite(monster, monsterData);

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                if (monsterNav != null)
                {
                    monsterNav.ApplyMoveSpeed(monsterData.moveSpeed);
                    monsterNav.SetUp();
                }
            }
            else
            {
                Debug.LogError($"PoolManager에서 {poolId}로 몬스터를 가져올 수 없습니다!");
            }
        }

        await UniTask.Delay(15000);
    }
}