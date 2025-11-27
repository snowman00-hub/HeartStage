using Unity.VisualScripting;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData; // SO를 직접 참조 (런타임 변경사항 즉시 반영)
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;
    private bool isBoss = false;
    private MonsterSpawner monsterSpawner;
    private HealthBar healthBar;
    public bool isDead = false;

    private readonly string attack = "Attack";
    private bool wasMoving = false; // 클래스 상단에 추가

    [SerializeField] private GameObject heartPrefab;
    private float fadeOutTime = 0.7f;
    private bool isFading = false;
    private float fadeTimer = 0f;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalSpriteColors;

    private Animator animator;
    //혼란 전용 셀프 콜라이더
    private Collider2D selfCollider;

    // 이동 상태 추적용
    private MonsterMovement monsterMovement;
    private Vector3 lastPosition;


    private int currentHP;
    private int maxHP; // 최대 HP는 따로 저장 (SO 변경 시에도 유지)

    public int GetCurrentHP() => currentHP;
    public MonsterData GetMonsterData() => monsterData;
    public bool IsBossMonster() => isBoss;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        monsterMovement = GetComponent<MonsterMovement>();
        lastPosition = transform.position;
    }

    // 몬스터 초기화 (SO 참조 설정, HP는 필요시에만 갱신)
    public void Init(MonsterData data)
    {
        monsterData = data;
        isDead = false;

        if (heartPrefab != null)
        {
            heartPrefab.SetActive(false);
        }

        if (selfCollider != null)
        {
            selfCollider.enabled = true;
        }

        // 최초 스폰 시 또는 최대 HP가 변경된 경우에만 HP 설정
        if (currentHP <= 0 || maxHP != data.hp)
        {
            maxHP = data.hp;
            currentHP = data.hp;
        }

        isBoss = IsBossMonster(data.id);
        InitHealthBar();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        lastPosition = transform.position;

        ActivateVisual();

        SaveOriginalColor();
        ResetFadeState();
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
        if (isFading)
        {
            UpdateFade();
            return; 
        }

        if (isDead || monsterData == null || EffectBase.Has<StunEffect>(gameObject))
            return;

        // 이동 상태 체크 및 애니메이션 업데이트
        UpdateMovementAnimation();

        // SO의 최신 공격속도 값을 직접 사용 (런타임 변경사항 즉시 반영)
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            if (IsTargetInRange())
            {
                if (EffectBase.Has<ConfuseEffect>(gameObject))
                {
                    ConfuseAttack();
                }
                else
                {
                    Attack();
                }
                attackCooldown = monsterData.attackSpeed; // SO에서 직접 가져옴
            }
        }
    }

    // 이동 상태에 따른 애니메이션 업데이트
    private void UpdateMovementAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        bool isCurrentlyMoving = Vector3.Distance(transform.position, lastPosition) > 0.01f;

        wasMoving = isCurrentlyMoving;
        lastPosition = transform.position;
    }

    // 공격 범위 내에 타겟이 있는지 확인
    private bool IsTargetInRange()
    {
        if (EffectBase.Has<ConfuseEffect>(gameObject))
        {
            // 혼란 상태일 때는 다른 몬스터 탐지
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                monsterData.attackRange,
                LayerMask.GetMask(Tag.Monster)
            );

            foreach (var hit in hits)
            {
                if (hit != null && hit != selfCollider)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // 일반 상태일 때는 벽 탐지
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position,
                monsterData.attackRange,
                LayerMask.GetMask(Tag.Wall)
            );
            return hit != null;
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

    public void OnDamage(int damage, bool isCritical = false)
    {
        if (isDead || isFading)
            return;

        if (monsterData != null)
        {
            currentHP -= damage;

            var ondamageEvents = GetComponents<IDamaged>();
            foreach (var ondamageEvent in ondamageEvents)
            {
                ondamageEvent.OnDamaged(damage, gameObject, isCritical);
            }
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if(selfCollider != null)
        {
            selfCollider.enabled = false;
        }

        if(heartPrefab != null)
        {
            heartPrefab.SetActive(true);
        }

        if (monsterSpawner != null && monsterData != null)
        {
            monsterSpawner.OnMonsterDied(monsterData.id);
        }

        isFading = true;
        fadeTimer = 0f;

        // 경험치 생성
        int rand = Random.Range(monsterData.minExp, monsterData.maxExp + 1);
        ItemManager.Instance.SpawnItem(ItemID.Exp, rand, transform.position);
        // 드랍아이템 생성
        if (monsterData == null)
            return;

        var dropList = DataTableManager.MonsterTable.GetDropItemInfo(monsterData.id);
        foreach (var dropItem in dropList)
        {
            ItemManager.Instance.SpawnItem(dropItem.Key, dropItem.Value, transform.position);
        }
    }

    private void MeleeAttack()
    {
        // 이미 Update에서 타겟이 있다고 확인했으므로, 바로 공격 실행
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(attack);
        }

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
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(attack);
        }

        Vector3 direction = GetAttackDirectionStageType();

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

    private Vector3 GetAttackDirectionStageType()
    {
        if (StageManager.Instance == null)
        {
            return Vector3.down; // 기본값
        }

        var currentStageData = StageManager.Instance.GetCurrentStageData();
        if (currentStageData == null)
        {
            return Vector3.down; // 기본값
        }

        // 스테이지 포지션에 따라 공격 방향 결정
        switch (currentStageData.stage_position)
        {
            case 1: 
                return Vector3.up;
            case 2: 
                return ToCenterAttack();

            case 3:
                return Vector3.down;

            default:
                return Vector3.down;
        }
    }

    // 중앙일때 공격 방향
    private Vector3 ToCenterAttack()
    {
        float currentY = transform.position.y;
        float centerY = 0f; 

        if (currentY > centerY)
        {
            return Vector3.down; 
        }
        else
        {
            return Vector3.up;  
        }
    }

    private void ConfuseAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        monsterData.attackRange,
        LayerMask.GetMask(Tag.Monster)
    );
        Collider2D targetCollider = null;

        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            // 자기 자신 콜라이더는 스킵
            if (hit == selfCollider)
                continue;

            targetCollider = hit;
            break; // 일단 하나만 때릴 거면 첫 번째만 선택
        }

        if (targetCollider != null)
        {
            if (animator != null)
            {
                animator.SetTrigger(attack);
            }

            var target = targetCollider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterData.att);
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

        return id == 22201 || id == 22214 || id == 22224; // 보스 id
    }

    private void ResetFadeState()
    {
        isFading = false;
        fadeTimer = 0f;

        // 원본 색상으로 복원
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalSpriteColors[i];
            }
        }
    }

    // 페이드 아웃으로 없어진 후에 이미지 활성화
    private void ActivateVisual()
    {
        if (monsterData != null && !string.IsNullOrEmpty(monsterData.prefab1))
        {
            Transform visualChild = transform.Find(monsterData.prefab1);
            if (visualChild != null)
            {
                visualChild.gameObject.SetActive(true);

                // 스프라이트 복원
                var childRenderers = visualChild.GetComponentsInChildren<SpriteRenderer>();
                foreach (var renderer in childRenderers)
                {
                    renderer.color = Color.gray;
                }
            }
        }
    }

    // 색상 원래대로 저장
    private void SaveOriginalColor()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalSpriteColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalSpriteColors[i] = spriteRenderers[i].color;
        }
    }

    private void UpdateFade()
    {
        fadeTimer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color newColor = originalSpriteColors[i];
                newColor.a = alpha;
                spriteRenderers[i].color = newColor;
            }
        }

        if (fadeTimer >= fadeOutTime)
        {
            gameObject.SetActive(false);
        }
    }
}