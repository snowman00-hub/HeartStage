using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

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
            Debug.Log("DeceptionSkill 실행");
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

                try
                {
                    var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_111011");
                    var monsterData = await handle.Task;

                    if (monsterData != null)
                    {
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

                        Debug.Log($"DeceptionSkill 몬스터 소환 성공: {i + 1}번째");
                    }
                    else
                    {
                        Debug.LogError("MonsterData_111011 로드 실패!");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"DeceptionSkill MonsterData 로드 오류: {e.Message}");
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