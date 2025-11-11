using UnityEngine;
using System.Collections.Generic;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData;
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;

    private List<IBossMonsterSkill> bossSkillList = new List<IBossMonsterSkill>();
    private bool isBoss = false;
    private float skillCoolTime = 15f;

    public void Init(MonsterData data)
    {
        monsterData = data;
        isBoss = IsBossMonster(data.id);

        if (isBoss)
        {
            InitializeBossSkills(data.id);
        }
    }

    private void Update()
    {
        if (monsterData == null)
            return;

        // 공격 쿨다운
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = monsterData.attackSpeed;
        }

        // 보스 스킬
        if (isBoss)
        {
            skillCoolTime -= Time.deltaTime;
            if (skillCoolTime <= 0f && bossSkillList.Count > 0)
            {
                UseBossSkills();
                skillCoolTime = 15f;
            }
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

    private bool IsBossMonster(int id)
    {
        return id == 121042;
    }

    private void InitializeBossSkills(int bossId)
    {
        switch (bossId)
        {
            case 121042:
                bossSkillList.Add(new DeceptionBossSKill(bossId.ToString(), 5));
                break;
        }
    }

    private void UseBossSkills()
    {
        foreach (var skill in bossSkillList)
        {
            skill.useSkill(this);
            Debug.Log($"보스 스킬 : {skill} 사용");
        }
    }
}