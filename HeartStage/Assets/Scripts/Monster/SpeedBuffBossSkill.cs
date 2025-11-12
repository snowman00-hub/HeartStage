using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class SpeedBuffBossSkill : MonoBehaviour, ISkillBehavior
{
    [Header("Reference")]

    [SerializeField] private float buffMultiplier = 1.3f; 
    [SerializeField] private float buffDuration = 5.5f;

    private Dictionary<GameObject, float> originalSpeeds = new Dictionary<GameObject, float>();

    private void Awake()
    {
        // 기본값 설정
        if (buffMultiplier <= 1f)
            buffMultiplier = 1.3f;

        if (buffDuration <= 0f)
            buffDuration = 5.5f;
    }

    public void Execute()
    {
        Debug.Log($"SpeedBuffBossSkill 실행 - 배수: {buffMultiplier}x, 지속시간: {buffDuration}초");

        // Physics.OverlapSphere로 몬스터들 직접 찾기
        var colliders = Physics.OverlapSphere(transform.position, 1000f, LayerMask.GetMask("Monster"));
        var monsters = new List<GameObject>();

        foreach (var collider in colliders)
        {
            var monsterBehavior = collider.GetComponent<MonsterBehavior>();
            if (monsterBehavior != null && collider.gameObject != this.gameObject)
            {
                monsters.Add(collider.gameObject);
            }
        }

        SpeedBuffEffect(monsters, buffDuration).Forget();
    }

    private async UniTaskVoid SpeedBuffEffect(List<GameObject> monsters, float duration)
    {
        originalSpeeds.Clear();

        foreach (var monster in monsters)
        {
            if (monster.activeInHierarchy)
            {
                var navAgent = monster.GetComponent<MonsterNavMeshAgent>();
                if (navAgent != null)
                {
                    float currentSpeed = navAgent.GetComponent<UnityEngine.AI.NavMeshAgent>().speed;
                    originalSpeeds[monster] = currentSpeed;

                    // 버프된 속도 적용
                    float buffedSpeed = currentSpeed * buffMultiplier;
                    navAgent.ApplyMoveSpeed(buffedSpeed);

                    Debug.Log($"몬스터 {monster.name} 속도 버프: {currentSpeed} → {buffedSpeed}");
                }
            }
        }

        await UniTask.Delay((int)(duration * 1000));

        // 원래 속도로 복구
        foreach (var kvp in originalSpeeds)
        {
            var monster = kvp.Key;
            var originalSpeed = kvp.Value;

            if (monster != null && monster.activeInHierarchy)
            {
                var navAgent = monster.GetComponent<MonsterNavMeshAgent>();
                if (navAgent != null)
                {
                    navAgent.ApplyMoveSpeed(originalSpeed);
                    Debug.Log($"몬스터 {monster.name} 속도 복구: {originalSpeed}");
                }
            }
        }
        originalSpeeds.Clear();
    }
}