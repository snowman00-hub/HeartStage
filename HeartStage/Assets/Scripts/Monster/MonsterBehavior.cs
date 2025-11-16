using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData; // SO를 직접 참조 (런타임 변경사항 즉시 반영)
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;
    private bool isBoss = false;
    private MonsterSpawner monsterSpawner;
    private HealthBar healthBar;

    private int currentHP;
    private int maxHP; // 최대 HP는 따로 저장 (SO 변경 시에도 유지)

    public int GetCurrentHP() => currentHP;
    public MonsterData GetMonsterData() => monsterData;
    public bool IsBossMonster() => isBoss;

    // 몬스터 초기화 (SO 참조 설정, HP는 필요시에만 갱신)
    public void Init(MonsterData data)
    {
        monsterData = data;

        // 최초 스폰 시 또는 최대 HP가 변경된 경우에만 HP 설정
        if (currentHP <= 0 || maxHP != data.hp)
        {
            maxHP = data.hp;
            currentHP = data.hp;
        }

        isBoss = IsBossMonster(data.id);
        InitHealthBar();
    }

    // 체력바 초기화
    private void InitHealthBar()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.Init(this, isBoss);
            healthBar.ShowHealthBar();
        }
    }

    public void SetMonsterSpawner(MonsterSpawner spawner)
    {
        monsterSpawner = spawner;
    }

    private void Update()
    {
        if (monsterData == null)
            return;

        // SO의 최신 공격속도 값을 직접 사용 (런타임 변경사항 즉시 반영)
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = monsterData.attackSpeed; // SO에서 직접 가져옴
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
        }
    }

    public void OnDamage(int damage)
    {
        if (monsterData != null)
        {
            currentHP -= damage;
            SoundManager.Instance.PlayMonsterHitSound();
            //Debug.Log($"{monsterData.monsterName}이(가) {damage}의 피해를 입었습니다. 남은 HP: {currentHP}");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (monsterSpawner != null && monsterData != null)
        {
            monsterSpawner.OnMonsterDied(monsterData.id);
        }

        gameObject.SetActive(false);
    }

    private void MeleeAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if (hit != null)
        {
            var target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterData.att); 
            }
        }
    }

    private void RangedAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if (hit != null)
        {
            Vector3 direction = Vector3.down;

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

    public static bool IsBossMonster(int id)
    {
        if (DataTableManager.MonsterTable != null)
        {
            var monsterData = DataTableManager.MonsterTable.Get(id);
            if (monsterData != null)
            {
                return monsterData.mon_type == 2; 
            }
        }

        return id == 22201 || id == 22214; // 보스 id
    }
}