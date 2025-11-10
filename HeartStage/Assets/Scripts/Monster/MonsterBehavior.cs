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

    public void Init(MonsterData data)
    {
        monsterData = data;
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
        // 몬스터 Die 애니메이션 처리
        //isAlive = false;
        gameObject.SetActive(false);
        Debug.Log("몬스터가 사망했습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            var agent = GetComponent<MonsterNavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }

            Attack();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            var agent = GetComponent<MonsterNavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }
        }
    }

    private void MeleeAttack()
    {
        // 애니메이션 처리
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

                var projectile = GetComponent<MonsterProjectile>();
                if (projectile != null)
                {
                    projectile.Init(direction, monsterData.bulletSpeed);
                }

                projectileObj.SetActive(true);
            }
        }
    }
}