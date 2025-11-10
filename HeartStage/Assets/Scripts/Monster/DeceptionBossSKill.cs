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

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                if (monsterBehavior != null)
                {
                    var monsterData = ScriptableObject.CreateInstance<MonsterData>();
                    monsterData.Init(111011); // test
                    monsterBehavior.Init(monsterData);
                }

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                if (monsterNav != null)
                {
                    var bossNav = boss.GetComponent<MonsterNavMeshAgent>();
                    if (bossNav != null && bossNav.targetPoints != null)
                    {
                        monsterNav.targetPoints = bossNav.targetPoints;
                    }
                    monsterNav.SetUp();

                    var navMeshAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (navMeshAgent != null && monsterBehavior != null)
                    {
                        var monsterData = ScriptableObject.CreateInstance<MonsterData>();
                        monsterData.Init(111011);
                        navMeshAgent.speed = monsterData.moveSpeed;
                    }
                }

                monster.SetActive(true);
            }
        }

        await UniTask.Delay(150000);
    }


}
