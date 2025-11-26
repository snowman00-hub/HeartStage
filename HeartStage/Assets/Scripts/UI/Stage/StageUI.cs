using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageUI : MonoBehaviour
{
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI remainMonsterCountText;

    [Header("Button")]
    [SerializeField] private Button BossSkill1Button1;
    [SerializeField] private Button BossSkill1Button2;
    [SerializeField] private Button BossSkill1Button3;

    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private int testMonsterId = 21101; // 기본 몬스터 ID (필요시 변경 가능)
    [SerializeField] private int spawnCount = 10; // 소환할 몬스터 수

    // 디버프 관리용 딕셔너리 추가
    private Dictionary<CharacterAttack, (float originalSpeed, float endTime)> attackSpeedDebuffs = new Dictionary<CharacterAttack, (float, float)>();

    private void Start()
    {
        // 버튼 이벤트 연결
        if (BossSkill1Button1 != null)
            BossSkill1Button1.onClick.AddListener(OnBossSkillButton1Clicked);

        if (BossSkill1Button2 != null)
            BossSkill1Button2.onClick.AddListener(OnBossSkillButton2Clicked);

        if (BossSkill1Button3 != null)
            BossSkill1Button3.onClick.AddListener(OnBossSkillButton3Clicked);
    }

    private void Update()
    {
        // 디버프 만료 체크
        CheckAttackSpeedDebuffExpiration();
    }

    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (BossSkill1Button1 != null)
            BossSkill1Button1.onClick.RemoveListener(OnBossSkillButton1Clicked);

        if (BossSkill1Button2 != null)
            BossSkill1Button2.onClick.RemoveListener(OnBossSkillButton2Clicked);

        if (BossSkill1Button3 != null)
            BossSkill1Button3.onClick.RemoveListener(OnBossSkillButton3Clicked);

        // 모든 디버프 해제
        ClearAllAttackSpeedDebuffs();
    }

    public void SetWaveCount(int stageNumber, int waveOrder)
    {
        if (stageNumber == 0)
        {
            waveCountText.text = $"Tutorial\nWave {waveOrder}";
        }
        else
        {
            var currentStage = StageManager.Instance.GetCurrentStageData();
            if (currentStage != null)
            {
                waveCountText.text = $"{currentStage.stage_step1}-{currentStage.stage_step2}스테이지\nWave {waveOrder}";
            }
            else
            {
                // currentStage가 null일 경우 stageNumber를 직접 사용
                waveCountText.text = $"{stageNumber}-1스테이지\nWave {waveOrder}";
            }
        }
    }

    public void SetReaminMonsterCount(int remainMonsterCount)
    {
        remainMonsterCountText.text = $"{remainMonsterCount}";
    }

    private void OnBossSkillButton1Clicked()
    {
        ExecuteSummonTest(); // 여러마리 소환 테스트
    }

    private void OnBossSkillButton2Clicked()
    {
        ExecuteSpeedBuffSkill(); // 스피드 버프 스킬 (31201)
    }

    private void OnBossSkillButton3Clicked()
    {
        ExecuteBooingSkill(); // 야유 스킬 (31101)
    }

    private void ExecuteSummonTest()
    {
        // MonsterSpawner 참조 확인
        if (monsterSpawner == null)
        {
            return;
        }

        // 테스트 소환 실행
        monsterSpawner.SpawnTestMonsters(testMonsterId, spawnCount).Forget();

        Debug.Log($"테스트 소환 실행: 몬스터 ID {testMonsterId} x {spawnCount}마리");
    }

    /// 스피드 버프 스킬 직접 실행 (31201)
    private void ExecuteSpeedBuffSkill()
    {
        // 스피드 버프는 모든 몬스터에게 적용
        var monsters = GameObject.FindGameObjectsWithTag(Tag.Monster);

        foreach (var monster in monsters)
        {
            if (monster == null || !monster.activeInHierarchy)
                continue;

            var monsterBehavior = monster.GetComponent<MonsterBehavior>();
            if (monsterBehavior == null) continue;

            var monsterData = monsterBehavior.GetMonsterData();
            if (monsterData == null) continue;

            // 이동속도 2배 버프를 5초간 적용
            EffectRegistry.Apply(monster, 3010, 2.0f, 5.0f);
        }

        Debug.Log($"스피드 버프 스킬 실행! {monsters.Length}마리 몬스터에게 적용");
    }

    /// 야유 스킬 직접 실행 (31101) - 코루틴 없이 개선된 버전
    private void ExecuteBooingSkill()
    {
        // 모든 소환된 캐릭터에게 공격속도 디버프 적용
        var characters = GameObject.FindGameObjectsWithTag(Tag.Tower);

        foreach (var character in characters)
        {
            if (character == null || !character.activeInHierarchy)
                continue;

            var characterAttack = character.GetComponent<CharacterAttack>();
            if (characterAttack == null) continue;

            // CSV에서 원본 공격속도 가져오기 (변조되지 않은 원본 값)
            var csvData = DataTableManager.CharacterTable.Get(characterAttack.id);
            if (csvData == null) continue;

            float originalSpeed = csvData.atk_speed; // CSV에서 직접 가져온 원본 값
            float debuffEndTime = Time.time + 5f; // 5초간 지속

            // 기존 디버프가 있으면 제거
            if (attackSpeedDebuffs.ContainsKey(characterAttack))
            {
                RemoveAttackSpeedDebuff(characterAttack);
            }

            // CharacterData에 디버프 적용
            var characterData = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
            if (characterData != null)
            {
                characterData.atk_speed = originalSpeed * 1.2f; //  느리게 

                // 디버프 정보 저장
                attackSpeedDebuffs[characterAttack] = (originalSpeed, debuffEndTime);
            }
        }

        Debug.Log($"야유 스킬 실행! {characters.Length}명 캐릭터의 공격속도 디버프 적용");
    }

    /// 디버프 만료 체크 (Update에서 호출)
    private void CheckAttackSpeedDebuffExpiration()
    {
        var expiredCharacters = new List<CharacterAttack>();

        foreach (var kvp in attackSpeedDebuffs)
        {
            var character = kvp.Key;
            var endTime = kvp.Value.endTime;

            // 캐릭터가 파괴되었거나 디버프 시간이 만료된 경우
            if (character == null || Time.time >= endTime)
            {
                expiredCharacters.Add(character);
            }
        }

        // 만료된 디버프들을 제거
        foreach (var character in expiredCharacters)
        {
            RemoveAttackSpeedDebuff(character);
        }
    }

    /// 개별 캐릭터의 공격속도 디버프 해제
    private void RemoveAttackSpeedDebuff(CharacterAttack character)
    {
        if (character == null || !attackSpeedDebuffs.ContainsKey(character))
            return;

        var originalSpeed = attackSpeedDebuffs[character].originalSpeed;

        // CharacterData 복구
        var csvData = DataTableManager.CharacterTable.Get(character.id);
        if (csvData != null)
        {
            var characterData = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
            if (characterData != null)
            {
                characterData.atk_speed = originalSpeed; // 원본 속도로 복구
            }
        }

        // 디버프 정보 제거
        attackSpeedDebuffs.Remove(character);
    }

    /// 모든 공격속도 디버프 해제
    private void ClearAllAttackSpeedDebuffs()
    {
        var allCharacters = new List<CharacterAttack>(attackSpeedDebuffs.Keys);
        foreach (var character in allCharacters)
        {
            RemoveAttackSpeedDebuff(character);
        }
    }
}