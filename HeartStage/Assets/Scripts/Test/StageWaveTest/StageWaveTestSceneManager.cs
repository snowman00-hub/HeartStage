using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageWaveTestSceneManager : MonoBehaviour
{
    // ==========================
    // 데이터 DB
    // ==========================
    private Dictionary<int, StageData> stageDB;
    private List<StageData> stageList;

    private Dictionary<int, StageWaveData> waveDB;

    private Dictionary<int, MonsterData> monsterDB;
    private List<MonsterData> monsterList;

    private StageData currentStage;
    private StageWaveData currentWave;
    private int currentWaveIndex = -1; // 0~3 (Wave1~4)

    // ==========================
    // Stage 선택 영역
    // ==========================
    [Header("Stage 선택")]
    [SerializeField] private TMP_Dropdown stageDropdown;
    [SerializeField] private TMP_InputField stageIdInput;

    [Header("Wave 버튼 (1~4)")]
    [SerializeField] private Button[] waveButtons;            // size = 4
    [SerializeField] private TMP_Text[] waveButtonLabels;     // size = 4

    // ==========================
    // Stage 편집 패널
    // ==========================
    [Header("Stage 편집 패널")]
    [SerializeField] private GameObject stagePanelRoot;

    [SerializeField] private TMP_InputField stageNameInput;
    [SerializeField] private TMP_InputField stageStep1Input;
    [SerializeField] private TMP_InputField stageStep2Input;
    [SerializeField] private TMP_InputField stageTypeInput;
    [SerializeField] private TMP_InputField stagePositionInput;
    [SerializeField] private TMP_InputField levelMaxInput;
    [SerializeField] private TMP_InputField memberCountInput;
    [SerializeField] private TMP_InputField dispatchMemberInput;
    [SerializeField] private TMP_InputField debutStaminaInput;
    [SerializeField] private TMP_InputField regularStaminaInput;
    [SerializeField] private TMP_InputField waveTimeInput;
    [SerializeField] private TMP_InputField wave1IdInput;
    [SerializeField] private TMP_InputField wave2IdInput;
    [SerializeField] private TMP_InputField wave3IdInput;
    [SerializeField] private TMP_InputField wave4IdInput;
    [SerializeField] private TMP_InputField dispatchRewardInput;
    [SerializeField] private TMP_InputField failStaminaInput;
    [SerializeField] private TMP_InputField stagePrefabInput;

    // ==========================
    // Wave 편집 패널
    // ==========================
    [Header("Wave 편집 패널")]
    [SerializeField] private GameObject wavePanelRoot;
    [SerializeField] private TMP_Text waveTitleText;         // "Wave 1 (ID:1234)"

    [SerializeField] private TMP_InputField waveNameInput;
    [SerializeField] private TMP_InputField enemySpawnTimeInput;

    // 몬스터 ID / Count (숫자 인풋은 그대로 두고, ID는 드롭다운이랑 동기화)
    [Header("Wave - Enemy ID / Count")]
    [SerializeField] private TMP_InputField enemyId1Input;
    [SerializeField] private TMP_InputField enemyCount1Input;
    [SerializeField] private TMP_InputField enemyId2Input;
    [SerializeField] private TMP_InputField enemyCount2Input;
    [SerializeField] private TMP_InputField enemyId3Input;
    [SerializeField] private TMP_InputField enemyCount3Input;

    [Header("Wave - Enemy 드롭다운 & 설정 버튼")]
    [SerializeField] private TMP_Dropdown enemy1Dropdown;
    [SerializeField] private TMP_Dropdown enemy2Dropdown;
    [SerializeField] private TMP_Dropdown enemy3Dropdown;

    [SerializeField] private Button enemy1EditButton;
    [SerializeField] private Button enemy2EditButton;
    [SerializeField] private Button enemy3EditButton;

    [Header("Wave 기타")]
    [SerializeField] private TMP_InputField waveRewardInput;
    [SerializeField] private TMP_InputField waveInfoInput;

    [SerializeField] private Button backToStageButton;

    // 몬스터 설정 패널
    [Header("몬스터 설정 패널")]
    [SerializeField] private MonsterTestPanel monsterPanel;

    // ==========================
    // 라이프사이클
    // ==========================

    private async void Awake()
    {
        SceneLoader.SetProgressExternal(1.0f);

        await UniTask.Delay(300, DelayType.UnscaledDeltaTime);

        GameSceneManager.NotifySceneReady(SceneType.TestStageScene, 100);
    }
    private async void Start()
    {
        // 1) 테이블 준비 대기
        while (DataTableManager.StageTable == null ||
               DataTableManager.StageWaveTable == null ||
               DataTableManager.MonsterTable == null)
        {
            await UniTask.Delay(10, DelayType.UnscaledDeltaTime);
        }

        // 2) 테이블에서 SO 딕셔너리 가져오기
        stageDB = DataTableManager.StageTable.GetAllData();          // Dictionary<int, StageData>
        waveDB = DataTableManager.StageWaveTable.GetAllData();      // Dictionary<int, StageWaveData>
        monsterDB = DataTableManager.MonsterTable.GetAll();          // Dictionary<int, MonsterData>

        stageList = new List<StageData>(stageDB.Values);
        stageList.Sort((a, b) => a.stage_ID.CompareTo(b.stage_ID));

        monsterList = new List<MonsterData>(monsterDB.Values);
        monsterList.Sort((a, b) => a.id.CompareTo(b.id));

        BuildStageDropdown();
        BuildMonsterDropdowns();
        HookEvents();

        ShowStagePanel();

        if (stageList.Count > 0)
        {
            OnStageChanged(0);
            if (stageDropdown != null)
                stageDropdown.SetValueWithoutNotify(0);
        }
    }

    // ==========================
    // 초기화 / 빌드
    // ==========================

    private void BuildStageDropdown()
    {
        if (stageDropdown == null) return;

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var s in stageList)
        {
            string label = $"{s.stage_ID} - {s.stage_name}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        stageDropdown.ClearOptions();
        stageDropdown.AddOptions(options);
    }

    private void BuildMonsterDropdowns()
    {
        BuildMonsterDropdown(enemy1Dropdown);
        BuildMonsterDropdown(enemy2Dropdown);
        BuildMonsterDropdown(enemy3Dropdown);
    }

    private void BuildMonsterDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null || monsterList == null) return;

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var m in monsterList)
        {
            string label = $"{m.id} - {m.monsterName}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    private void HookEvents()
    {
        // Stage 선택
        if (stageDropdown != null)
            stageDropdown.onValueChanged.AddListener(OnStageChanged);

        if (stageIdInput != null)
            stageIdInput.onEndEdit.AddListener(OnStageIdInputChanged);

        // Wave 버튼
        if (waveButtons != null && waveButtons.Length == 4)
        {
            for (int i = 0; i < waveButtons.Length; i++)
            {
                int idx = i;
                if (waveButtons[idx] != null)
                    waveButtons[idx].onClick.AddListener(() => OnWaveButtonClicked(idx));
            }
        }

        if (backToStageButton != null)
            backToStageButton.onClick.AddListener(OnBackToStageClicked);

        // Stage 필드 변경 이벤트
        if (stageNameInput != null) stageNameInput.onEndEdit.AddListener(OnStageNameChanged);
        if (stageStep1Input != null) stageStep1Input.onEndEdit.AddListener(OnStageStep1Changed);
        if (stageStep2Input != null) stageStep2Input.onEndEdit.AddListener(OnStageStep2Changed);
        if (stageTypeInput != null) stageTypeInput.onEndEdit.AddListener(OnStageTypeChanged);
        if (stagePositionInput != null) stagePositionInput.onEndEdit.AddListener(OnStagePositionChanged);
        if (levelMaxInput != null) levelMaxInput.onEndEdit.AddListener(OnLevelMaxChanged);
        if (memberCountInput != null) memberCountInput.onEndEdit.AddListener(OnMemberCountChanged);
        if (dispatchMemberInput != null) dispatchMemberInput.onEndEdit.AddListener(OnDispatchMemberChanged);
        if (debutStaminaInput != null) debutStaminaInput.onEndEdit.AddListener(OnDebutStaminaChanged);
        if (regularStaminaInput != null) regularStaminaInput.onEndEdit.AddListener(OnRegularStaminaChanged);
        if (waveTimeInput != null) waveTimeInput.onEndEdit.AddListener(OnWaveTimeChanged);

        if (wave1IdInput != null) wave1IdInput.onEndEdit.AddListener(t => OnWaveIdFieldChanged(0, t));
        if (wave2IdInput != null) wave2IdInput.onEndEdit.AddListener(t => OnWaveIdFieldChanged(1, t));
        if (wave3IdInput != null) wave3IdInput.onEndEdit.AddListener(t => OnWaveIdFieldChanged(2, t));
        if (wave4IdInput != null) wave4IdInput.onEndEdit.AddListener(t => OnWaveIdFieldChanged(3, t));

        if (dispatchRewardInput != null) dispatchRewardInput.onEndEdit.AddListener(OnDispatchRewardChanged);
        if (failStaminaInput != null) failStaminaInput.onEndEdit.AddListener(OnFailStaminaChanged);
        if (stagePrefabInput != null) stagePrefabInput.onEndEdit.AddListener(OnStagePrefabChanged);

        // Wave 필드 변경 이벤트
        if (waveNameInput != null) waveNameInput.onEndEdit.AddListener(OnWaveNameChanged);
        if (enemySpawnTimeInput != null) enemySpawnTimeInput.onEndEdit.AddListener(OnEnemySpawnTimeChanged);

        if (enemyId1Input != null) enemyId1Input.onEndEdit.AddListener(OnEnemyId1Changed);
        if (enemyCount1Input != null) enemyCount1Input.onEndEdit.AddListener(OnEnemyCount1Changed);
        if (enemyId2Input != null) enemyId2Input.onEndEdit.AddListener(OnEnemyId2Changed);
        if (enemyCount2Input != null) enemyCount2Input.onEndEdit.AddListener(OnEnemyCount2Changed);
        if (enemyId3Input != null) enemyId3Input.onEndEdit.AddListener(OnEnemyId3Changed);
        if (enemyCount3Input != null) enemyCount3Input.onEndEdit.AddListener(OnEnemyCount3Changed);

        if (waveRewardInput != null) waveRewardInput.onEndEdit.AddListener(OnWaveRewardChanged);
        if (waveInfoInput != null) waveInfoInput.onEndEdit.AddListener(OnWaveInfoChanged);

        // 몬스터 드롭다운 이벤트
        HookMonsterDropdownEvents();

        // 몬스터 설정 버튼
        if (enemy1EditButton != null)
            enemy1EditButton.onClick.AddListener(() => OpenMonsterEditorForSlot(1));
        if (enemy2EditButton != null)
            enemy2EditButton.onClick.AddListener(() => OpenMonsterEditorForSlot(2));
        if (enemy3EditButton != null)
            enemy3EditButton.onClick.AddListener(() => OpenMonsterEditorForSlot(3));
    }

    private void HookMonsterDropdownEvents()
    {
        if (enemy1Dropdown != null)
            enemy1Dropdown.onValueChanged.AddListener(i => OnEnemyDropdownChanged(1, i));
        if (enemy2Dropdown != null)
            enemy2Dropdown.onValueChanged.AddListener(i => OnEnemyDropdownChanged(2, i));
        if (enemy3Dropdown != null)
            enemy3Dropdown.onValueChanged.AddListener(i => OnEnemyDropdownChanged(3, i));
    }

    // ==========================
    // Stage 선택 / 동기화
    // ==========================

    private void OnStageChanged(int index)
    {
        if (index < 0 || index >= stageList.Count) return;

        currentStage = stageList[index];

        if (stageIdInput != null)
            stageIdInput.SetTextWithoutNotify(currentStage.stage_ID.ToString());

        SyncStageToUI();
        UpdateWaveButtons();
        ShowStagePanel();
    }

    private void OnStageIdInputChanged(string newText)
    {
        if (stageList == null || stageList.Count == 0) return;

        int fallback = currentStage != null ? currentStage.stage_ID : 0;
        int id = ParseInt(newText, fallback);

        if (currentStage != null && id == currentStage.stage_ID)
        {
            stageIdInput?.SetTextWithoutNotify(currentStage.stage_ID.ToString());
            return;
        }

        int idx = stageList.FindIndex(s => s.stage_ID == id);
        if (idx < 0)
        {
            Debug.LogWarning($"[StageWaveTest] stage_ID {id} 를 찾을 수 없습니다.");
            if (currentStage != null)
                stageIdInput?.SetTextWithoutNotify(currentStage.stage_ID.ToString());
            return;
        }

        if (stageDropdown != null)
            stageDropdown.SetValueWithoutNotify(idx);

        OnStageChanged(idx);
    }

    private void SyncStageToUI()
    {
        if (currentStage == null) return;

        stageNameInput?.SetTextWithoutNotify(currentStage.stage_name ?? "");
        stageStep1Input?.SetTextWithoutNotify(currentStage.stage_step1.ToString());
        stageStep2Input?.SetTextWithoutNotify(currentStage.stage_step2.ToString());
        stageTypeInput?.SetTextWithoutNotify(currentStage.stage_type.ToString());
        stagePositionInput?.SetTextWithoutNotify(currentStage.stage_position.ToString());
        levelMaxInput?.SetTextWithoutNotify(currentStage.level_max.ToString());
        memberCountInput?.SetTextWithoutNotify(currentStage.member_count.ToString());
        dispatchMemberInput?.SetTextWithoutNotify(currentStage.dispatch_member.ToString());
        debutStaminaInput?.SetTextWithoutNotify(currentStage.debut_stamina.ToString());
        regularStaminaInput?.SetTextWithoutNotify(currentStage.regular_stamina.ToString());
        waveTimeInput?.SetTextWithoutNotify(currentStage.wave_time.ToString());

        wave1IdInput?.SetTextWithoutNotify(currentStage.wave1_id.ToString());
        wave2IdInput?.SetTextWithoutNotify(currentStage.wave2_id.ToString());
        wave3IdInput?.SetTextWithoutNotify(currentStage.wave3_id.ToString());
        wave4IdInput?.SetTextWithoutNotify(currentStage.wave4_id.ToString());

        dispatchRewardInput?.SetTextWithoutNotify(currentStage.dispatch_reward.ToString());
        failStaminaInput?.SetTextWithoutNotify(currentStage.fail_stamina.ToString());
        stagePrefabInput?.SetTextWithoutNotify(currentStage.prefab ?? "");
    }

    private void UpdateWaveButtons()
    {
        if (currentStage == null) return;
        if (waveButtons == null || waveButtons.Length < 4) return;

        int[] waveIds =
        {
            currentStage.wave1_id,
            currentStage.wave2_id,
            currentStage.wave3_id,
            currentStage.wave4_id,
        };

        for (int i = 0; i < 4; i++)
        {
            var btn = waveButtons[i];
            if (btn == null) continue;

            int waveId = waveIds[i];
            bool exists = (waveId > 0) && waveDB.ContainsKey(waveId);

            btn.interactable = exists;
            btn.gameObject.SetActive(true);

            if (waveButtonLabels != null && waveButtonLabels.Length > i && waveButtonLabels[i] != null)
            {
                if (exists)
                    waveButtonLabels[i].text = $"Wave {i + 1}\nID: {waveId}";
                else
                    waveButtonLabels[i].text = $"Wave {i + 1}\n(없음)";
            }
        }
    }

    // ==========================
    // Wave 선택 / 동기화
    // ==========================

    private void OnWaveButtonClicked(int waveIndex) // 0~3
    {
        if (currentStage == null) return;

        int waveId = GetWaveIdByIndex(currentStage, waveIndex);
        if (waveId <= 0)
        {
            Debug.LogWarning($"[StageWaveTest] Wave {waveIndex + 1} 는 설정된 wave_id 가 없습니다.");
            return;
        }

        if (!waveDB.TryGetValue(waveId, out var waveData))
        {
            Debug.LogWarning($"[StageWaveTest] wave_id {waveId} 에 해당하는 StageWaveData SO를 찾을 수 없습니다.");
            return;
        }

        currentWaveIndex = waveIndex;
        currentWave = waveData;

        SyncWaveToUI();
        ShowWavePanel();
    }

    private int GetWaveIdByIndex(StageData stage, int index)
    {
        switch (index)
        {
            case 0: return stage.wave1_id;
            case 1: return stage.wave2_id;
            case 2: return stage.wave3_id;
            case 3: return stage.wave4_id;
        }
        return 0;
    }

    private void SyncWaveToUI()
    {
        if (currentWave == null) return;

        if (waveTitleText != null)
        {
            string waveNum = currentWaveIndex >= 0 ? (currentWaveIndex + 1).ToString() : "?";
            waveTitleText.text = $"Wave {waveNum} (ID: {currentWave.wave_id})";
        }

        waveNameInput?.SetTextWithoutNotify(currentWave.wave_name ?? "");
        enemySpawnTimeInput?.SetTextWithoutNotify(currentWave.enemy_spown_time.ToString("0.00", CultureInfo.InvariantCulture));

        enemyId1Input?.SetTextWithoutNotify(currentWave.EnemyID1.ToString());
        enemyCount1Input?.SetTextWithoutNotify(currentWave.EnemyCount1.ToString());
        enemyId2Input?.SetTextWithoutNotify(currentWave.EnemyID2.ToString());
        enemyCount2Input?.SetTextWithoutNotify(currentWave.EnemyCount2.ToString());
        enemyId3Input?.SetTextWithoutNotify(currentWave.EnemyID3.ToString());
        enemyCount3Input?.SetTextWithoutNotify(currentWave.EnemyCount3.ToString());

        waveRewardInput?.SetTextWithoutNotify(currentWave.wave_reward.ToString());
        waveInfoInput?.SetTextWithoutNotify(currentWave.info ?? "");

        SyncWaveMonsterDropdowns();
    }

    private void SyncWaveMonsterDropdowns()
    {
        if (currentWave == null || monsterList == null) return;

        SetMonsterDropdownValue(enemy1Dropdown, currentWave.EnemyID1);
        SetMonsterDropdownValue(enemy2Dropdown, currentWave.EnemyID2);
        SetMonsterDropdownValue(enemy3Dropdown, currentWave.EnemyID3);
    }

    private void SetMonsterDropdownValue(TMP_Dropdown dropdown, int monsterId)
    {
        if (dropdown == null || monsterList == null) return;

        int idx = monsterList.FindIndex(m => m.id == monsterId);
        if (idx < 0) idx = 0;

        dropdown.SetValueWithoutNotify(idx);
    }

    private void OnBackToStageClicked()
    {
        ShowStagePanel();
        SyncStageToUI();
    }

    // ==========================
    // Panel On/Off
    // ==========================

    private void ShowStagePanel()
    {
        if (stagePanelRoot != null) stagePanelRoot.SetActive(true);
        if (wavePanelRoot != null) wavePanelRoot.SetActive(false);
    }

    private void ShowWavePanel()
    {
        if (stagePanelRoot != null) stagePanelRoot.SetActive(false);
        if (wavePanelRoot != null) wavePanelRoot.SetActive(true);
    }

    // ==========================
    // Stage 변경 핸들러
    // ==========================

    private void OnStageNameChanged(string newText)
    {
        if (currentStage == null) return;
        currentStage.stage_name = newText;
        MarkStageDirty();
        BuildStageDropdown(); // 이름 바뀌면 드롭다운 갱신
    }

    private void OnStageStep1Changed(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.stage_step1);
        currentStage.stage_step1 = v;
        stageStep1Input?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnStageStep2Changed(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.stage_step2);
        currentStage.stage_step2 = v;
        stageStep2Input?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnStageTypeChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.stage_type);
        currentStage.stage_type = v;
        stageTypeInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnStagePositionChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.stage_position);
        currentStage.stage_position = v;
        stagePositionInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnLevelMaxChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.level_max);
        currentStage.level_max = v;
        levelMaxInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnMemberCountChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.member_count);
        currentStage.member_count = v;
        memberCountInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnDispatchMemberChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.dispatch_member);
        currentStage.dispatch_member = v;
        dispatchMemberInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnDebutStaminaChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.debut_stamina);
        currentStage.debut_stamina = v;
        debutStaminaInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnRegularStaminaChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.regular_stamina);
        currentStage.regular_stamina = v;
        regularStaminaInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnWaveTimeChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.wave_time);
        currentStage.wave_time = v;
        waveTimeInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnWaveIdFieldChanged(int index, string newText)
    {
        if (currentStage == null) return;

        int oldVal = GetWaveIdByIndex(currentStage, index);
        int v = ParseInt(newText, oldVal);

        SetWaveIdByIndex(currentStage, index, v);
        MarkStageDirty();
        UpdateWaveButtons();

        switch (index)
        {
            case 0: wave1IdInput?.SetTextWithoutNotify(v.ToString()); break;
            case 1: wave2IdInput?.SetTextWithoutNotify(v.ToString()); break;
            case 2: wave3IdInput?.SetTextWithoutNotify(v.ToString()); break;
            case 3: wave4IdInput?.SetTextWithoutNotify(v.ToString()); break;
        }
    }

    private void SetWaveIdByIndex(StageData stage, int index, int value)
    {
        switch (index)
        {
            case 0: stage.wave1_id = value; break;
            case 1: stage.wave2_id = value; break;
            case 2: stage.wave3_id = value; break;
            case 3: stage.wave4_id = value; break;
        }
    }

    private void OnDispatchRewardChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.dispatch_reward);
        currentStage.dispatch_reward = v;
        dispatchRewardInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnFailStaminaChanged(string newText)
    {
        if (currentStage == null) return;
        int v = ParseInt(newText, currentStage.fail_stamina);
        currentStage.fail_stamina = v;
        failStaminaInput?.SetTextWithoutNotify(v.ToString());
        MarkStageDirty();
    }

    private void OnStagePrefabChanged(string newText)
    {
        if (currentStage == null) return;
        currentStage.prefab = newText;
        MarkStageDirty();
    }

    // ==========================
    // Wave 변경 핸들러
    // ==========================

    private void OnWaveNameChanged(string newText)
    {
        if (currentWave == null) return;
        currentWave.wave_name = newText;
        MarkWaveDirty();
    }

    private void OnEnemySpawnTimeChanged(string newText)
    {
        if (currentWave == null) return;
        float v = ParseFloat(newText, currentWave.enemy_spown_time);
        currentWave.enemy_spown_time = v;
        enemySpawnTimeInput?.SetTextWithoutNotify(v.ToString("0.00", CultureInfo.InvariantCulture));
        MarkWaveDirty();
    }

    private void OnEnemyId1Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyID1);
        currentWave.EnemyID1 = v;
        enemyId1Input?.SetTextWithoutNotify(v.ToString());
        SyncWaveMonsterDropdowns();
        MarkWaveDirty();
    }

    private void OnEnemyCount1Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyCount1);
        currentWave.EnemyCount1 = v;
        enemyCount1Input?.SetTextWithoutNotify(v.ToString());
        MarkWaveDirty();
    }

    private void OnEnemyId2Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyID2);
        currentWave.EnemyID2 = v;
        enemyId2Input?.SetTextWithoutNotify(v.ToString());
        SyncWaveMonsterDropdowns();
        MarkWaveDirty();
    }

    private void OnEnemyCount2Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyCount2);
        currentWave.EnemyCount2 = v;
        enemyCount2Input?.SetTextWithoutNotify(v.ToString());
        MarkWaveDirty();
    }

    private void OnEnemyId3Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyID3);
        currentWave.EnemyID3 = v;
        enemyId3Input?.SetTextWithoutNotify(v.ToString());
        SyncWaveMonsterDropdowns();
        MarkWaveDirty();
    }

    private void OnEnemyCount3Changed(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.EnemyCount3);
        currentWave.EnemyCount3 = v;
        enemyCount3Input?.SetTextWithoutNotify(v.ToString());
        MarkWaveDirty();
    }

    private void OnWaveRewardChanged(string newText)
    {
        if (currentWave == null) return;
        int v = ParseInt(newText, currentWave.wave_reward);
        currentWave.wave_reward = v;
        waveRewardInput?.SetTextWithoutNotify(v.ToString());
        MarkWaveDirty();
    }

    private void OnWaveInfoChanged(string newText)
    {
        if (currentWave == null) return;
        currentWave.info = newText;
        MarkWaveDirty();
    }

    // ==========================
    // 몬스터 드롭다운 / 패널
    // ==========================

    private void OnEnemyDropdownChanged(int slotIndex, int dropdownIndex)
    {
        if (currentWave == null || monsterList == null) return;
        if (dropdownIndex < 0 || dropdownIndex >= monsterList.Count) return;

        int monsterId = monsterList[dropdownIndex].id;

        switch (slotIndex)
        {
            case 1:
                currentWave.EnemyID1 = monsterId;
                enemyId1Input?.SetTextWithoutNotify(monsterId.ToString());
                break;
            case 2:
                currentWave.EnemyID2 = monsterId;
                enemyId2Input?.SetTextWithoutNotify(monsterId.ToString());
                break;
            case 3:
                currentWave.EnemyID3 = monsterId;
                enemyId3Input?.SetTextWithoutNotify(monsterId.ToString());
                break;
        }

        MarkWaveDirty();
    }

    private void OpenMonsterEditorForSlot(int slotIndex)
    {
        if (currentWave == null || monsterDB == null) return;

        int monsterId = 0;
        switch (slotIndex)
        {
            case 1: monsterId = currentWave.EnemyID1; break;
            case 2: monsterId = currentWave.EnemyID2; break;
            case 3: monsterId = currentWave.EnemyID3; break;
        }

        if (monsterId <= 0 || !monsterDB.TryGetValue(monsterId, out var monster))
        {
            Debug.LogWarning($"[StageWaveTest] EnemyID{slotIndex} 몬스터가 없음. id={monsterId}");
            return;
        }

        if (monsterPanel != null)
        {
            monsterPanel.Open(monster, () =>
            {
                // 몬스터 패널 닫힐 때, 필요하면 Wave 다시 갱신
                SyncWaveToUI();
            });
        }
    }

    // ==========================
    // 공통 파서 / Dirty 표시
    // ==========================

    private int ParseInt(string text, int fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        if (int.TryParse(text, out int v)) return v;
        return fallback;
    }

    private float ParseFloat(string text, float fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        text = text.Replace(',', '.');
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;
        return fallback;
    }

    private void MarkStageDirty()
    {
#if UNITY_EDITOR
        if (currentStage != null)
            UnityEditor.EditorUtility.SetDirty(currentStage);
#endif
    }

    private void MarkWaveDirty()
    {
#if UNITY_EDITOR
        if (currentWave != null)
            UnityEditor.EditorUtility.SetDirty(currentWave);
#endif
    }
}
