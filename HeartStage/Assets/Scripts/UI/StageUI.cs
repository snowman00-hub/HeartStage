using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StageUI : MonoBehaviour
{
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI remainMonsterCountText;

    [Header("Button")]
    [SerializeField] private Button BossSkill1Button1;
    [SerializeField] private Button BossSkill1Button2;
    [SerializeField] private Button BossSkill1Button3;

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

    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (BossSkill1Button1 != null)
            BossSkill1Button1.onClick.RemoveListener(OnBossSkillButton1Clicked);

        if (BossSkill1Button2 != null)
            BossSkill1Button2.onClick.RemoveListener(OnBossSkillButton2Clicked);

        if (BossSkill1Button3 != null)
            BossSkill1Button3.onClick.RemoveListener(OnBossSkillButton3Clicked);
    }

    public void SetWaveCount(int stageNumber, int waveOrder)
    {
        if (stageNumber == 0)
        {
            waveCountText.text = $"Tutorial {waveOrder}";
        }
        else
        {
            var currentStage = StageManager.Instance.GetCurrentStageData();
            if (currentStage != null)
            {
                waveCountText.text = $"{currentStage.stage_step1}-{currentStage.stage_step2}스테이지\n{waveOrder}웨이브";
            }
            else
            {
                // currentStage가 null일 경우 stageNumber를 직접 사용
                waveCountText.text = $"{stageNumber}-1스테이지\n{waveOrder}웨이브";
            }
        }
    }

    public void SetReaminMonsterCount(int remainMonsterCount)
    {
        remainMonsterCountText.text = $"남은 적군\n{remainMonsterCount}";
    }

    private void OnBossSkillButton1Clicked()
    {
        ExecuteSummonSkill(); // 소환 스킬 (31001)
    }

    private void OnBossSkillButton2Clicked()
    {
        ExecuteSpeedBuffSkill(); // 스피드 버프 스킬 (31201)
    }

    private void OnBossSkillButton3Clicked()
    {
        ExecuteBooingSkill(); // 야유 스킬 (31101)
    }

    /// 소환 스킬 직접 실행 (31001)
    private void ExecuteSummonSkill()
    {
        // 임시 게임오브젝트 생성해서 소환 스킬 실행
        GameObject tempSkillCaster = new GameObject("TempSummonSkill");        

        float spawnX = Random.Range(-2f, 2f);
        var spownPos = new Vector3(spawnX, Screen.height - 100f, 0f);

        tempSkillCaster.transform.position = spownPos;

        // MonsterBehavior 컴포넌트도 추가 (DeceptionBossSkill이 필요로 함)
        var monsterBehavior = tempSkillCaster.AddComponent<MonsterBehavior>();
        
        // BossAddScript 컴포넌트도 추가 (DeceptionBossSkill.Execute()가 필요로 함)
        var bossAddScript = tempSkillCaster.AddComponent<BossAddScript>();

        // DeceptionBossSkill 컴포넌트 추가
        var summonSkill = tempSkillCaster.AddComponent<DeceptionBossSkill>();

        // 더미 몬스터 데이터 생성 (소환 스킬에 필요)
        var dummyMonsterData = ScriptableObject.CreateInstance<MonsterData>();
        dummyMonsterData.id = 22201; 

        // MonsterBehavior 초기화
        monsterBehavior.Init(dummyMonsterData);

        // 스킬 초기화 및 실행
        summonSkill.InitializeWithMonsterData(dummyMonsterData).ContinueWith(() =>
        {
            summonSkill.Execute();
        });

        Debug.Log("소환 스킬 실행!");

        // 10초 후 임시 오브젝트 삭제
        Destroy(tempSkillCaster, 10f);
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

    /// 야유 스킬 직접 실행 (31101)
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

            // CharacterData 가져오기
            var csvData = DataTableManager.CharacterTable.Get(characterAttack.id);
            if (csvData == null) continue;

            var characterData = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
            if (characterData == null) continue;

            // 공격속도를 10배 느리게 만들기 (5초간)
            float originalSpeed = characterData.atk_speed;
            characterData.atk_speed = originalSpeed * 10f;

            // 5초 후 원래 속도로 복구
            StartCoroutine(RestoreAttackSpeedAfterDelay(characterData, originalSpeed, 5f));
        }

        Debug.Log($"야유 스킬 실행! {characters.Length}명 캐릭터의 공격속도 디버프 적용");
    }

    /// 지정된 시간 후 공격속도 복구
    private System.Collections.IEnumerator RestoreAttackSpeedAfterDelay(CharacterData characterData, float originalSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (characterData != null)
        {
            characterData.atk_speed = originalSpeed;
        }
    }
}