using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 데일리 퀘스트 전체 관리:
/// - 오늘 날짜 기준 dailyQuest 리셋/유지
/// - QuestTable에서 Quest_type == Daily 인 퀘스트 자동 수집
/// - 진행도(progress) / 진행도 보상(QuestProgressTable) / 완료 상태 저장
/// - SaveLoadManager.Data.dailyQuest 사용
/// </summary>
public class DailyQuests : MonoBehaviour
{
    #region 내부 클래스 - 진행도 보상 슬롯

    [Serializable]
    private class RewardButtonSlot
    {
        [Header("UI")]
        public Button button;
        public Image iconImage;

        [Header("QuestProgressTable.csv 의 progress_reward_ID")]
        public int progressRewardId;

        [NonSerialized] public QuestProgressData data;

        [NonSerialized] public Sprite notFilledSprite;
        [NonSerialized] public Sprite filledSprite;
        [NonSerialized] public Sprite claimedSprite;
    }

    #endregion

    [Header("진행도 게이지")]
    [SerializeField] private Slider progressSlider;

    [Header("진행도 보상 버튼 슬롯들 (5개)")]
    [SerializeField] private RewardButtonSlot[] rewardSlots = new RewardButtonSlot[5];

    [Header("데일리 퀘스트 리스트 (ScrollView Content)")]
    [SerializeField] private Transform questListContent;       // ScrollView/Viewport/Content
    [SerializeField] private DailyQuestItemUI questItemPrefab; // 프리팹

    [Header("이 진행도는 어떤 타입인가? (Daily 추천)")]
    [SerializeField] private ProgressType progressType = ProgressType.Daily;

    // DataTableManager 통해 접근 (BootStrap에서 Initialization 끝난 상태라고 가정)
    private QuestTable QuestTable => DataTableManager.QuestTable;
    private QuestProgressTable QuestProgressTable => DataTableManager.QuestProgressTable;

    // SaveDataV1 안에 있는 DailyQuestState
    private DailyQuestState State => SaveLoadManager.Data.dailyQuest;

    private readonly List<QuestData> _dailyQuestList = new List<QuestData>();
    private readonly List<DailyQuestItemUI> _questItems = new List<DailyQuestItemUI>();

    #region Unity Lifecycle

    private async void Start()
    {
        InitStateStructure();

        // 1) 서버 기준 오늘 날짜 문자열
        DateTime serverNow;
        try
        {
            serverNow = FirebaseTime.GetServerTime();
        }
        catch
        {
            serverNow = DateTime.Now;
        }

        string todayKey = serverNow.ToString("yyyyMMdd");

        // 2) 날짜 다르면 → 새 날로 간주하고 리셋
        if (State.date != todayKey)
        {
            ResetDailyState(todayKey);
            await SaveDailyStateAsync();
        }

        // 3) 슬라이더 세팅
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = 100;
            progressSlider.wholeNumbers = true;
            progressSlider.value = State.progress;
        }

        // 4) Daily 퀘스트 목록 생성 (Quest_type == Daily)
        BuildDailyQuestList();

        // 5) 진행도 보상 버튼 세팅 + 아이콘 로드
        HookRewardButtonEvents();
        InitQuestProgressDataFromTable();
        await LoadRewardIconsAsync();

        // 6) 진행도/보상 버튼 상태 반영
        UpdateRewardButtons();

        // 7) 스크롤 뷰에 퀘스트 UI 생성 + 완료 상태 반영
        CreateDailyQuestItems();
        ApplyCompletedStateToItems();

        Debug.Log($"[DailyQuests] date={State.date}, progress={State.progress}, completed={State.completedQuestIds.Count}");
    }
    private void OnEnable()
    {
        QuestManager.DailyQuestCompleted -= OnDailyQuestClearedExternally; // 중복 방지용
        QuestManager.DailyQuestCompleted += OnDailyQuestClearedExternally;

        RefreshAllItemStatesFromSave();
    }

    private void OnDisable()
    {

        QuestManager.DailyQuestCompleted -= OnDailyQuestClearedExternally;

    }

    #endregion

    #region DailyQuestState 초기화 / 리셋 / 저장

    private void InitStateStructure()
    {
        if (SaveLoadManager.Data.dailyQuest == null)
            SaveLoadManager.Data.dailyQuest = new DailyQuestState();

        if (State.claimed == null || State.claimed.Length != rewardSlots.Length)
            State.claimed = new bool[rewardSlots.Length];

        if (State.completedQuestIds == null)
            State.completedQuestIds = new List<int>();
    }

    private void ResetDailyState(string todayKey)
    {
        InitStateStructure();

        State.date = todayKey;
        State.progress = 0;

        Array.Clear(State.claimed, 0, State.claimed.Length);
        State.completedQuestIds.Clear();
    }

    private async UniTask SaveDailyStateAsync()
    {
        await SaveLoadManager.SaveToServer();
    }
    private void RefreshAllItemStatesFromSave()
    {
        if (_questItems == null || _questItems.Count == 0)
            return;

        if (State.clearedQuestIds == null)
            State.clearedQuestIds = new List<int>();
        if (State.completedQuestIds == null)
            State.completedQuestIds = new List<int>();

        foreach (var item in _questItems)
        {
            if (item == null)
                continue;

            int id = item.QuestId;

            bool completed = State.completedQuestIds.Contains(id);
            bool cleared = completed || State.clearedQuestIds.Contains(id);

            item.SetState(cleared, completed);
        }
    }
    #endregion

    #region Daily 퀘스트 목록 만들기 (Quest_type == Daily 자동 수집)

    /// <summary>
    /// QuestTable에서 Quest_type == QuestType.Daily 인 퀘스트만 모아서 내부 리스트 구성
    /// </summary>
    private void BuildDailyQuestList()
    {
        _dailyQuestList.Clear();

        var table = QuestTable;
        if (table == null)
        {
            Debug.LogError("[DailyQuests] QuestTable 이 null 입니다.");
            return;
        }

        // 네가 QuestTable에 만든 GetByType(QuestType type) 기준
        foreach (var q in table.GetByType(QuestType.Daily))
        {
            if (q == null)
                continue;

            _dailyQuestList.Add(q);
        }

        // 필요하면 ID 순 정렬
        _dailyQuestList.Sort((a, b) => a.Quest_ID.CompareTo(b.Quest_ID));
    }

    #endregion

    #region 진행도 보상 슬롯 초기화 / 아이콘 로드

    private void HookRewardButtonEvents()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            int index = i;

            if (slot?.button == null)
                continue;

            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() => OnClickRewardButton(index));
        }
    }

    private void InitQuestProgressDataFromTable()
    {
        var qpt = QuestProgressTable;
        if (qpt == null)
        {
            Debug.LogError("[DailyQuests] QuestProgressTable 이 null 입니다.");
            return;
        }

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null)
                continue;

            if (slot.progressRewardId == 0)
            {
                Debug.LogWarning($"[DailyQuests] rewardSlots[{i}] 의 progressRewardId 가 0 입니다. CSV의 progress_reward_ID 를 인스펙터에 넣어줘야 함.");
                continue;
            }

            QuestProgressData data = qpt.Get(slot.progressRewardId);
            if (data == null)
            {
                Debug.LogError($"[DailyQuests] QuestProgressData 찾기 실패. progress_reward_ID = {slot.progressRewardId}");
                continue;
            }

            // progress_type: enum ProgressType (CSV int) vs 설정 progressType
            if (data.progress_type != progressType)
            {
                Debug.LogWarning($"[DailyQuests] 슬롯 {i} progress_type({data.progress_type}) != 설정 progressType({progressType})");
            }

            slot.data = data;
        }
    }

    private async UniTask LoadRewardIconsAsync()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null || slot.data == null || slot.iconImage == null)
                continue;

            try
            {
                // Notfill_icon
                if (!string.IsNullOrEmpty(slot.data.Notfill_icon))
                {
                    var h = Addressables.LoadAssetAsync<Sprite>(slot.data.Notfill_icon);
                    slot.notFilledSprite = await h.Task;
                }

                // filled_icon
                if (!string.IsNullOrEmpty(slot.data.filled_icon))
                {
                    var h = Addressables.LoadAssetAsync<Sprite>(slot.data.filled_icon);
                    slot.filledSprite = await h.Task;
                }

                // get_reward_icon
                if (!string.IsNullOrEmpty(slot.data.get_reward_icon))
                {
                    var h = Addressables.LoadAssetAsync<Sprite>(slot.data.get_reward_icon);
                    slot.claimedSprite = await h.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DailyQuests] 진행도 보상 아이콘 로드 실패 (slot {i}, id {slot.progressRewardId}) : {ex}");
            }
        }
    }

    #endregion

    #region 진행도 / 보상 버튼 로직

    public int CurrentProgress => State.progress;

    public void AddProgress(int delta)
    {
        int next = Mathf.Clamp(State.progress + delta, 0, 100);
        SetProgress(next);
    }

    public async void SetProgress(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        State.progress = value;

        if (progressSlider != null)
            progressSlider.value = State.progress;

        UpdateRewardButtons();
        await SaveDailyStateAsync();
    }

    private void UpdateRewardButtons()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null || slot.button == null || slot.iconImage == null || slot.data == null)
                continue;

            bool claimed = State.claimed != null && i < State.claimed.Length && State.claimed[i];
            int needProgress = slot.data.progress_amount;

            if (claimed)
            {
                slot.button.interactable = false;
                if (slot.claimedSprite != null)
                    slot.iconImage.sprite = slot.claimedSprite;
            }
            else
            {
                if (State.progress >= needProgress)
                {
                    slot.button.interactable = true;
                    if (slot.filledSprite != null)
                        slot.iconImage.sprite = slot.filledSprite;
                }
                else
                {
                    slot.button.interactable = false;
                    if (slot.notFilledSprite != null)
                        slot.iconImage.sprite = slot.notFilledSprite;
                }
            }
        }
    }

    // ★ 버튼에서 직접 쓰는 콜백은 얘 하나
    private void OnClickRewardButton(int index)
    {
        ClaimRewardInternal(index).Forget();
    }

    // ★ 실제 로직은 여기로 몰아넣음 → AllReceive에서도 재사용
    private async UniTask ClaimRewardInternal(int index)
    {
        if (index < 0 || index >= rewardSlots.Length)
            return;

        var slot = rewardSlots[index];
        if (slot == null || slot.data == null)
            return;

        if (State.claimed != null && index < State.claimed.Length && State.claimed[index])
        {
            // 이미 수령
            return;
        }

        if (State.progress < slot.data.progress_amount)
        {
            Debug.Log("[DailyQuests] 진행도 부족, 보상 수령 불가");
            return;
        }

        // 실제 보상 지급 (TODO: 아이템 지급 시스템 연결)
        GiveProgressReward(slot.data);

        if (State.claimed == null || State.claimed.Length != rewardSlots.Length)
            State.claimed = new bool[rewardSlots.Length];

        State.claimed[index] = true;

        UpdateRewardButtons();
        await SaveDailyStateAsync();
    }

    /// <summary>
    /// [전체 보상 받기]에서 호출할 함수
    /// - 현재 진행도 조건을 만족하지만 아직 안 받은 진행도 보상을 전부 수령
    /// </summary>
    public void ClaimAllAvailableRewards()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            // 조건(진행도/이미 수령 여부)은 ClaimRewardInternal 안에서 다시 체크하니까 그냥 돌리면 됨
            ClaimRewardInternal(i).Forget();
        }
    }

    private void GiveProgressReward(QuestProgressData data)
    {
        // TODO: 여기서 ItemInvenHelper.AddItem(...) 등으로 실제 보상 지급
        if (data.reward1 != 0 && data.reward1_amount > 0)
            Debug.Log($"[DailyQuests] Reward1: {data.reward1} x {data.reward1_amount}");
        if (data.reward2 != 0 && data.reward2_amount > 0)
            Debug.Log($"[DailyQuests] Reward2: {data.reward2} x {data.reward2_amount}");
        if (data.reward3 != 0 && data.reward3_amount > 0)
            Debug.Log($"[DailyQuests] Reward3: {data.reward3} x {data.reward3_amount}");
    }

    #endregion

    #region ScrollView 안 데일리 퀘스트 생성 / 완료 처리

    private void CreateDailyQuestItems()
    {
        if (questListContent == null || questItemPrefab == null)
        {
            Debug.LogWarning("[DailyQuests] Quest 리스트 생성에 필요한 참조(questListContent, questItemPrefab)가 없습니다.");
            return;
        }

        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        _questItems.Clear();

        if (_dailyQuestList.Count == 0)
        {
            Debug.LogWarning("[DailyQuests] Daily 타입 퀘스트가 없습니다. QuestTable / Quest_type 확인.");
            return;
        }

        for (int i = 0; i < _dailyQuestList.Count; i++)
        {
            var data = _dailyQuestList[i];
            int id = data.Quest_ID;

            bool completed = State.completedQuestIds != null &&
                             State.completedQuestIds.Contains(id);

            bool cleared = false;

            // SaveData 기준으로 조건 충족 여부
            if (State.clearedQuestIds != null &&
                State.clearedQuestIds.Contains(id))
            {
                cleared = true;
            }

            // 이미 보상까지 받은 퀘스트면 당연히 cleared도 true
            if (completed)
                cleared = true;

            var item = Instantiate(questItemPrefab, questListContent);
            item.Init(this, data, cleared, completed);

            _questItems.Add(item);
        }
    }


    private void ApplyCompletedStateToItems()
    {
        if (State.completedQuestIds == null)
            return;

        foreach (var item in _questItems)
        {
            if (item == null)
                continue;

            if (State.completedQuestIds.Contains(item.QuestId))
                item.SetCompleted(true);
        }
    }

    /// <summary>
    /// UI에서 "완료" 버튼 눌렀을 때 호출됨.
    /// - 조건은 이미 QuestManager 에서 만족된 상태라고 가정.
    /// - 여기서 보상 지급 + 진행도 증가 + completed 목록에 등록.
    /// </summary>
    public async void OnQuestItemClickedComplete(QuestData questData, DailyQuestItemUI itemUI)
    {
        if (questData == null || itemUI == null)
            return;

        if (State.completedQuestIds == null)
            State.completedQuestIds = new List<int>();

        int id = questData.Quest_ID;
        bool alreadyCompleted = State.completedQuestIds.Contains(id);

        if (!alreadyCompleted)
        {
            State.completedQuestIds.Add(id);

            // TODO: 여기서 퀘스트 개별 보상 지급(Quest_reward1~3)도 처리하면 됨.
            // ex) ItemManager.AddItem(questData.Quest_reward1, questData.Quest_reward1_A);

            // 진행도 타입이 현재 Daily 진행도 타입과 일치하면 progress 증가
            if (questData.progress_type == (int)progressType &&
                questData.progress_amount > 0)
            {
                AddProgress(questData.progress_amount);
                // AddProgress 안에서 SaveDailyStateAsync 호출함
            }
            else
            {
                await SaveDailyStateAsync();
            }
        }

        // UI는 무조건 "조건 충족 + 보상 수령 완료" 상태로 맞춰준다
        itemUI.SetState(cleared: true, completed: true);
    }

    /// <summary>
    /// 전투/로비 등 외부 시스템에서 "조건을 만족했다"는 신호를 받을 때 호출됨.
    /// </summary>
    public void OnDailyQuestClearedExternally(QuestData questData)
    {
        if (questData == null)
            return;

        if (questData.Quest_type != QuestType.Daily)
            return;

        int id = questData.Quest_ID;

        if (State.clearedQuestIds == null)
            State.clearedQuestIds = new List<int>();

        if (!State.clearedQuestIds.Contains(id))
            State.clearedQuestIds.Add(id);

        var ui = _questItems.Find(x => x.QuestId == id);
        if (ui != null)
        {
            bool completed = State.completedQuestIds != null &&
                             State.completedQuestIds.Contains(id);

            ui.SetState(cleared: true, completed: completed);
        }

        // 조건 충족 상태는 저장해두는 편이 좋음 (앱 껐다 켜도 유지)
        SaveDailyStateAsync().Forget();
    }

    #endregion
}
public class DailyQuestState
{
    // 마지막으로 갱신된 날짜 (서버 기준) "yyyyMMdd"
    public string date;

    // 진행도 (0~100)
    public int progress;

    // 진행도 보상 5개 수령 여부
    public bool[] claimed = new bool[5];

    // 오늘 조건을 만족한(클리어된) 데일리 퀘스트 ID 목록 (보상은 아직 안 받았을 수 있음)
    public List<int> clearedQuestIds = new List<int>();

    // 오늘 보상까지 받은 데일리 퀘스트 ID 목록
    public List<int> completedQuestIds = new List<int>();
}