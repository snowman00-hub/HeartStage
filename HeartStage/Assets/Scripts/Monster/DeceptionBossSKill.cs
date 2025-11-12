using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DeceptionBossSKill : IBossMonsterSkill
{
    private int spawnCount; // test
    private string poolId;

    public DeceptionBossSKill(string poolId, int spawnCount = 5)
    {
        this.poolId = poolId;
        this.spawnCount = spawnCount;
    }

    public void useSkill(MonsterBehavior boss)
    {
        DeceptionSkill(boss).Forget();
    }

    public async UniTaskVoid DeceptionSkill(MonsterBehavior boss)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = boss.transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = boss.transform.position.y;

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

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                if (monsterNav != null)
                {
                    monsterNav.ApplyMoveSpeed(monsterData.moveSpeed);
                    monsterNav.SetUp();
                }
            }
        }

        await UniTask.Delay(150000);
    }
}