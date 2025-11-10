using System.Text;
using UnityEditor;
using UnityEngine;
public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Reference")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Field")]
    private MonsterData monsterData;
    private const string MonsterProjectilePoolId = "MonsterProjectile"; // 임시 아이디 
    float attackCooldown = 0;
    public void Init(MonsterData data)
    {
        monsterData = data;
    }

    private void Update()
    {
        if (monsterData == null)
            return;

        MonsterMoveControll();

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = monsterData.attackSpeed;
        }
    }

    public void Attack()
    {     
        switch (monsterData.attType)
        {
            case 1:
                MeleeAttack();
                break;
            case 2:
                RangedAttack();
                break;
            default:               
                break;
        }
    }

    public void OnDamage(int damage)
    {
        if (monsterData != null)
        {
            monsterData.hp -= damage;
        }

        if (monsterData.hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("몬스터가 사망했습니다.");
    }

    private void MeleeAttack()
    {
        // 애니메이션 처리
        Debug.Log($"attackRange: {monsterData.attackRange}");
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if(hit != null)
        {
            var target = hit.GetComponent<IDamageable>();
            if(target != null)
            {
               target.OnDamage(monsterData.att);
            }
        }
    }

    private void RangedAttack()
    {
        // 애니메이션 처리
        Debug.Log($"attackRange: {monsterData.attackRange}");
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));
        
        if (hit != null)
        {
            Vector3 targetPosition = hit.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;

            var projectileObj = PoolManager.Instance.Get(MonsterProjectilePoolId);
            if (projectileObj != null)
            {
                projectileObj.transform.position = transform.position;
                projectileObj.transform.rotation = Quaternion.identity;

                var projectile = projectileObj.GetComponent<MonsterProjectile>();
                if (projectile != null)
                {
                    projectile.Init(direction, monsterData.bulletSpeed, monsterData.att);
                }

                projectileObj.SetActive(true);
            }
        }
    }

    private bool IsEnemyInRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));
        return hit != null;
    }

    private void MonsterMoveControll()
    {
        bool enemyInRange = IsEnemyInRange();
        if (enemyInRange)
        {
            StopMove();
        }
        else
        {
            ResumeMove();
        }
    }

    private void StopMove()
    {
        var agent = GetComponent<MonsterNavMeshAgent>();
        if (agent != null && agent.isChasingPlayer)
        {
            agent.isChasingPlayer = false;
            agent.ClearTarget();

            var nav  = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null)
            {
                nav.isStopped = true;
                nav.ResetPath();
            }
        }
    }

    private void ResumeMove()
    {
        var agent = GetComponent<MonsterNavMeshAgent>();
        if (agent != null && !agent.isChasingPlayer)
        {
            agent.isChasingPlayer = true;
            agent.RestoreTarget();

            var nav = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null)
            {
                nav.isStopped = false;                
            }
        }
    }

}