using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class BuffBossSkill : IBossMonsterSkill
{
    Dictionary<GameObject, float> originalSpeeds = new Dictionary<GameObject, float>();

    public void useSkill(MonsterBehavior boss)
    {
        // 보스가 자신을 포함한 모든 몬스터들의 이동속도를 3초간 30% 증가시킨다.
        
    }

    private async UniTask SpeedBuffEffect(MonsterSpawner spawnedMonster, float duration)
    {
        foreach(var monster in spawnedMonster.MonsterList)
        {
            var monsterBehavior = monster.GetComponent<MonsterBehavior>();
            var navAgent = monster.GetComponent<MonsterNavMeshAgent>();

            if (monsterBehavior != null && navAgent != null)
            {
                //float originalSpeed = monsterBehavior.monsterData.moveSpeed;
                //navAgent.ApplyMoveSpeed(originalSpeed * 1.3f);
            }
        }

        await UniTask.Delay((int)duration * 1000);

    }
}
