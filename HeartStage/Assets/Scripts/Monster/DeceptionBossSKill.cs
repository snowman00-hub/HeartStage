using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class DeceptionBossSkill : MonoBehaviour, ISkillBehavior
{
    [SerializeField] private int spawnCount = 5;
    private string poolId = "21101"; // test

    private void Awake()
    {
        if (string.IsNullOrEmpty(poolId))
            poolId = "21101"; // test

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
                    var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_21101"); // test
                    var monsterData = await handle.Task;

                    if (monsterData != null)
                    {
                        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                        if (monsterBehavior != null)
                        {
                            monsterBehavior.Init(monsterData);
                        }

                        MonsterSpawner.SetMonsterSprite(monster, monsterData);

                        var monsterMovement = monster.GetComponent<MonsterMovement>();

                        if(monsterMovement != null)
                        {
                            monsterMovement.Init(monsterData, Vector3.down);
                        }

                        Debug.Log($"DeceptionSkill 몬스터 소환 성공: {i + 1}번째");
                    }

                }
                catch (System.Exception e)
                {
                    Debug.LogError($"DeceptionSkill MonsterData 로드 오류: {e.Message}");
                }
            }
        }

        await UniTask.Delay(15000);
    }
}