using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ArchivementQuests : MonoBehaviour, IQuestItemOwner
{
    [Header("스크롤뷰 컨텐츠 (업적 퀘스트 아이템들이 들어갈 부모)")]
    [SerializeField] private Transform questListContent;

    [Header("업적 퀘스트 아이템 프리팹")]
    [SerializeField] private ArchivementQuestItemUI questItemPrefab;

    private readonly List<QuestData> _achievementQuestList = new List<QuestData>();
    private readonly List<ArchivementQuestItemUI> _questItems = new List<ArchivementQuestItemUI>();

    private QuestTable QuestTable => DataTableManager.Get<QuestTable>(DataTableIds.Quest);

    /// <summary>SaveLoadManager.Data 쪽에 있는 업적 상태</summary>
    private AchievementQuestState State
    {
        get
        {
            if (SaveLoadManager.Data.achievementQuest == null)
                SaveLoadManager.Data.achievementQuest = new AchievementQuestState();
            return SaveLoadManager.Data.achievementQuest;
        }
    }

    public bool IsInitialized { get; private set; }

    private async void OnEnable()
    {
        if (!IsInitialized)
        {
            await InitializeAsync();
        }
        else
        {
            // 이미 초기화된 상태면, 세이브 기준으로 UI만 갱신
            RefreshAllItemStatesFromSave();
        }
    }

    /// <summary>
    /// 업적 탭 초기화:
    /// - SaveData 구조 보정
    /// - QuestTable에서 업적 퀘스트 목록 가져오기
    /// - 스크롤뷰에 아이템 생성 + 상태 반영
    /// </summary>
    public async UniTask InitializeAsync()
    {
        InitStateStructure();
        BuildAchievementQuestList();
        CreateAchievementQuestItems();
        RefreshAllItemStatesFromSave();

        IsInitialized = true;

        await UniTask.CompletedTask;
    }

    private void InitStateStructure()
    {
        if (SaveLoadManager.Data.achievementQuest == null)
            SaveLoadManager.Data.achievementQuest = new AchievementQuestState();

        if (State.clearedQuestIds == null)
            State.clearedQuestIds = new List<int>();

        if (State.completedQuestIds == null)
            State.completedQuestIds = new List<int>();
    }

    /// <summary>
    /// QuestTable에서 Quest_type == Achievement 인 애들만 모아옴
    /// </summary>
    private void BuildAchievementQuestList()
    {
        _achievementQuestList.Clear();

        var table = QuestTable;
        if (table == null)
        {
            Debug.LogError("[ArchivementQuests] QuestTable 이 null 입니다.");
            return;
        }

        // ★ 여기서 QuestType.Achievement 는 네 enum 이름에 맞춰서 수정해줘
        foreach (var q in table.GetByType(QuestType.Achievement))
        {
            if (q == null)
                continue;

            _achievementQuestList.Add(q);
        }

        // ID 순 정렬 (원하면)
        _achievementQuestList.Sort((a, b) => a.Quest_ID.CompareTo(b.Quest_ID));
    }

    /// <summary>
    /// 스크롤뷰에 업적 퀘스트 아이템 생성
    /// </summary>
    private void CreateAchievementQuestItems()
    {
        // 이전 것들 정리
        if (_questItems != null)
        {
            foreach (var item in _questItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _questItems.Clear();
        }

        if (questItemPrefab == null || questListContent == null)
        {
            Debug.LogWarning("[ArchivementQuests] questItemPrefab 또는 questListContent 가 비어있습니다.");
            return;
        }

        foreach (var data in _achievementQuestList)
        {
            int id = data.Quest_ID;
            bool completed = State.completedQuestIds != null && State.completedQuestIds.Contains(id);
            bool cleared = completed ||
                           (State.clearedQuestIds != null && State.clearedQuestIds.Contains(id));

            var item = Instantiate(questItemPrefab, questListContent);
            item.Init(this, data, cleared, completed);

            _questItems.Add(item);
        }
    }

    /// <summary>
    /// SaveData 기준으로 모든 아이템 상태 갱신
    /// (창 다시 열었을 때 등)
    /// </summary>
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

    /// <summary>
    /// IQuestItemOwner 구현:
    /// - 업적 퀘스트 아이템에서 "완료" 버튼 눌렀을 때 호출
    /// - 여기서 실제 보상 지급 + completed 등록 + 세이브
    /// </summary>
    public async void OnQuestItemClickedComplete(QuestData questData, QuestItemUIBase itemUI)
    {
        if (questData == null || itemUI == null)
            return;

        if (State.completedQuestIds == null)
            State.completedQuestIds = new List<int>();

        int id = questData.Quest_ID;

        // 이미 보상까지 받은 업적이면 무시
        if (State.completedQuestIds.Contains(id))
            return;

        State.completedQuestIds.Add(id);

        // TODO: 여기서 업적 개별 보상 지급
        GiveQuestReward(questData);

        await SaveAchievementStateAsync();

        // UI는 무조건 "조건 충족 + 보상 수령 완료" 상태로 맞춰줌
        itemUI.SetState(cleared: true, completed: true);
    }

    /// <summary>
    /// 나중에 QuestManager에서 "이 업적 조건 만족했다" 라고 알려줄 때 호출할 함수
    /// </summary>
    public void OnAchievementQuestClearedExternally(QuestData questData)
    {
        if (questData == null)
            return;

        // 업적 타입만 받기
        if (questData.Quest_type != QuestType.Achievement)
            return;

        int id = questData.Quest_ID;

        if (State.clearedQuestIds == null)
            State.clearedQuestIds = new List<int>();

        if (!State.clearedQuestIds.Contains(id))
            State.clearedQuestIds.Add(id);

        // 이미 생성된 UI가 있으면 즉시 반영
        var ui = _questItems.Find(x => x.QuestId == id);
        if (ui != null)
        {
            bool completed = State.completedQuestIds != null &&
                             State.completedQuestIds.Contains(id);
            ui.SetState(cleared: true, completed: completed);
        }

        // 조건 충족 상태는 저장해두는 편이 좋음
        SaveAchievementStateAsync().Forget();
    }

    /// <summary>
    /// [전체 보상 받기] (업적 탭용)
    /// - "조건은 만족했는데 아직 보상 안 받은 업적" 전부 수령
    /// </summary>
    public void ClaimAllAvailableRewards()
    {
        if (_questItems == null)
            return;

        foreach (var item in _questItems)
        {
            if (item == null)
                continue;

            int id = item.QuestId;

            // 이미 보상 받은 업적이면 스킵
            if (State.completedQuestIds != null && State.completedQuestIds.Contains(id))
                continue;

            // 조건 자체를 만족 안 했으면 스킵
            bool cleared = State.clearedQuestIds != null &&
                           State.clearedQuestIds.Contains(id);
            if (!cleared)
                continue;

            // QuestData 찾아서 동일 로직 재사용
            var questData = _achievementQuestList.Find(q => q.Quest_ID == id);
            if (questData != null)
            {
                OnQuestItemClickedComplete(questData, item);
            }
        }
    }

    private void GiveQuestReward(QuestData questData)
    {
        // TODO: 실제 아이템 지급 시스템 연결 (Daily랑 같은 패턴으로)
        if (questData.Quest_reward1 != 0 && questData.Quest_reward1_A > 0)
            Debug.Log($"[ArchivementQuests] Reward1: {questData.Quest_reward1} x {questData.Quest_reward1_A}");

        if (questData.Quest_reward2 != 0 && questData.Quest_reward2_A > 0)
            Debug.Log($"[ArchivementQuests] Reward2: {questData.Quest_reward2} x {questData.Quest_reward2_A}");

        if (questData.Quest_reward3 != 0 && questData.Quest_reward3_A > 0)
            Debug.Log($"[ArchivementQuests] Reward3: {questData.Quest_reward3} x {questData.Quest_reward3_A}");
    }

    private async UniTask SaveAchievementStateAsync()
    {
        await SaveLoadManager.SaveToServer();
    }
}


// ==========================
//  업적용 세이브 상태 구조체
// ==========================
[System.Serializable]
public class AchievementQuestState
{
    // 조건을 만족한 업적 퀘스트 (보상은 아직 안 받았을 수 있음)
    public List<int> clearedQuestIds = new List<int>();

    // 보상까지 받은 업적 퀘스트
    public List<int> completedQuestIds = new List<int>();
}
