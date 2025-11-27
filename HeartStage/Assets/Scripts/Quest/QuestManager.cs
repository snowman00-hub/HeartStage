using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 퀘스트 전담 논-UI 매니저.
/// - DontDestroyOnLoad 싱글톤
/// - SaveLoadManager.Data.dailyQuest 를 사용해서 Daily 퀘스트 상태 관리
/// - 게임 전역 이벤트(출석, 스테이지 클리어, 몬스터 처치, 뽑기, 상점 구매)를 받아서
///   해당 Daily 퀘스트 완료 + 진행도(progress) 증가 처리
/// - 나중에 Weekly / Achievement 도 같은 패턴으로 확장 가능
/// </summary>
public class QuestManager : MonoBehaviour
{
    #region Singleton

    public static QuestManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    /// <summary>
    /// 게임 전체에서 쓸 “이벤트 타입” 태그.
    /// CSV랑 숫자를 맞추는 게 아니라, 코드 안에서만 어떤 이벤트인지 구분용으로 쓴다.
    /// </summary>
    public enum DailyQuestEventType
    {
        None = 0,
        Attendance = 1, // 출석
        ClearStage = 2, // 스테이지 클리어
        MonsterKill = 3, // 몬스터 처치
        GachaDraw = 4, // 뽑기
        ShopPurchase = 5, // 상점 구매
    }

    [Header("Daily 퀘스트 ID 매핑 (QuestData.Quest_ID)")]
    [Tooltip("출석 체크 데일리 퀘스트 Quest_ID")]
    public int attendanceDailyQuestId;

    [Tooltip("일일 스테이지 1회 클리어 데일리 퀘스트 Quest_ID")]
    public int clearStageDailyQuestId;

    [Tooltip("몬스터 처치 데일리 퀘스트 Quest_ID")]
    public int monsterKillDailyQuestId;

    [Tooltip("뽑기 1회 데일리 퀘스트 Quest_ID")]
    public int gachaDrawDailyQuestId;

    [Tooltip("상점 구매 1회 데일리 퀘스트 Quest_ID")]
    public int shopPurchaseDailyQuestId;

    private QuestTable QuestTable => DataTableManager.Get<QuestTable>(DataTableIds.Quest);
    private QuestProgressTable QuestProgressTable => DataTableManager.Get<QuestProgressTable>(DataTableIds.QuestProgress);
    private QuestTypeTable QuestTypeTable => DataTableManager.Get<QuestTypeTable>(DataTableIds.QuestType);

    // 내부 캐시: 오늘 활성화된 Daily 퀘스트 리스트
    private readonly List<QuestData> _dailyQuestList = new List<QuestData>();
    private readonly HashSet<int> _completedDailyQuestIds = new HashSet<int>();

    private bool _initializedDaily;

    /// <summary>
    /// BootStrap에서:
    /// - DataTableManager.Initialization
    /// - SaveLoadManager.LoadFromServer()
    /// 끝난 이후 한 번만 호출해주면 됨.
    /// </summary>
    public void Initialize()
    {
        InitializeDailyQuests();
        // 나중에 Weekly / Achievement 도 여기서 같이 초기화하면 됨.
    }

    #region Daily 상태 접근자

    private DailyQuestState DailyState
    {
        get
        {
            if (SaveLoadManager.Data.dailyQuest == null)
                SaveLoadManager.Data.dailyQuest = new DailyQuestState();
            return SaveLoadManager.Data.dailyQuest;
        }
    }

    /// <summary>오늘 일일 진행도 (0~100)</summary>
    public int DailyProgress => DailyState.progress;

    /// <summary>해당 Quest_ID가 오늘 이미 완료되었는지?</summary>
    public bool IsDailyQuestCompleted(int questId) => _completedDailyQuestIds.Contains(questId);

    /// <summary>오늘 활성화된 Daily 퀘스트 정의 리스트 (UI에서 사용)</summary>
    public IReadOnlyList<QuestData> DailyQuests => _dailyQuestList;

    #endregion

    #region Daily 초기화 / 리셋

    private void InitializeDailyQuests()
    {
        if (_initializedDaily)
            return;

        InitDailyStateAndDate();
        BuildDailyQuestList();
        SyncDailyCompletedSet();

        _initializedDaily = true;

        Debug.Log($"[QuestManager] DailyQuests 초기화 완료. date={DailyState.date}, progress={DailyState.progress}, count={_dailyQuestList.Count}");
    }

    private void InitDailyStateAndDate()
    {
        // 구조 보장
        var state = DailyState;

        if (state.claimed == null || state.claimed.Length == 0)
            state.claimed = new bool[5];

        if (state.completedQuestIds == null)
            state.completedQuestIds = new List<int>();

        // 날짜 체크 (FirebaseTime → 실패시 로컬 시간)
        DateTime now;
        try
        {
            now = FirebaseTime.GetServerTime();
        }
        catch
        {
            now = DateTime.Now;
        }

        string todayKey = now.ToString("yyyyMMdd");

        if (string.IsNullOrEmpty(state.date) || state.date != todayKey)
        {
            ResetDailyState(todayKey);
            // 하루 초기화는 바로 저장해도 크게 부담 없음
            SaveLoadManager.SaveToServer().Forget();
        }
    }

    private void ResetDailyState(string todayKey)
    {
        var state = DailyState;

        state.date = todayKey;
        state.progress = 0;

        if (state.claimed == null || state.claimed.Length == 0)
            state.claimed = new bool[5];

        Array.Clear(state.claimed, 0, state.claimed.Length);

        state.completedQuestIds.Clear();
        _completedDailyQuestIds.Clear();

        Debug.Log($"[QuestManager] DailyQuest 리셋. date={todayKey}");
    }

    private void BuildDailyQuestList()
    {
        _dailyQuestList.Clear();

        var table = QuestTable;
        if (table == null)
        {
            Debug.LogError("[QuestManager] QuestTable 이 null 입니다.");
            return;
        }

        // ★ QuestTable에 아래 메서드 하나 추가해두면 됨:
        //   public IEnumerable<QuestData> GetByType(QuestType type) { ... }
        foreach (QuestData q in table.GetByType(QuestType.Daily))
        {
            if (q == null)
                continue;

            _dailyQuestList.Add(q);
        }

        _dailyQuestList.Sort((a, b) => a.Quest_ID.CompareTo(b.Quest_ID));
    }

    private void SyncDailyCompletedSet()
    {
        _completedDailyQuestIds.Clear();

        var state = DailyState;

        if (state.completedQuestIds == null)
            state.completedQuestIds = new List<int>();

        foreach (int id in state.completedQuestIds)
        {
            _completedDailyQuestIds.Add(id);
        }
    }

    private void EnsureDailyInitialized()
    {
        if (!_initializedDaily)
        {
            InitializeDailyQuests();
        }
    }

    #endregion

    #region 외부에서 호출할 이벤트 진입점 (Daily)

    // BootStrap.UpdateLastLoginTime() 쪽에서,
    // "오늘 처음 접속" 판정이 날 때 한 번 호출해주면 됨.
    public void OnAttendance()
    {
        EnsureDailyInitialized();
        TryCompleteDailyById(attendanceDailyQuestId, DailyQuestEventType.Attendance);
    }

    // 스테이지 클리어 시점(StageManager 등)에서 호출
    public void OnStageClear(int stageId)
    {
        EnsureDailyInitialized();
        // 필요하면 stageId로 특정 스테이지만 인정하는 조건 추가 가능
        TryCompleteDailyById(clearStageDailyQuestId, DailyQuestEventType.ClearStage);
    }

    // 몬스터 사망 시점(Monster / MonsterHP 등)에서 호출
    public void OnMonsterKilled(int monsterId)
    {
        EnsureDailyInitialized();
        // 특정 몬스터만 카운트할 거면 여기서 monsterId 체크 가능
        TryCompleteDailyById(monsterKillDailyQuestId, DailyQuestEventType.MonsterKill);
    }

    // 가챠 결과 확정 시점에서 호출 (count: 1회/10회 등)
    public void OnGachaDraw(int count)
    {
        EnsureDailyInitialized();
        // 10연차 1번도 인정이면 그냥 1번만 호출, count 로 세분화하려면 나중에 로직 추가
        TryCompleteDailyById(gachaDrawDailyQuestId, DailyQuestEventType.GachaDraw);
    }

    // 상점 구매 성공 시점에서 호출 (shopItemId: 상점 상품 id)
    public void OnShopPurchase(int shopItemId)
    {
        EnsureDailyInitialized();
        // 특정 상품만 인정이면 shopItemId 체크해서 필터 가능
        TryCompleteDailyById(shopPurchaseDailyQuestId, DailyQuestEventType.ShopPurchase);
    }

    #endregion

    #region Daily 퀘스트 완료 처리

    private void TryCompleteDailyById(int questId, DailyQuestEventType evtType)
    {
        if (questId <= 0)
        {
            Debug.LogWarning($"[QuestManager] {evtType} 에 매핑된 Daily Quest_ID 가 설정되지 않았습니다.");
            return;
        }

        var table = QuestTable;
        if (table == null)
        {
            Debug.LogError("[QuestManager] QuestTable 이 null 입니다.");
            return;
        }

        QuestData quest = table.Get(questId);
        if (quest == null)
        {
            Debug.LogError($"[QuestManager] Quest_ID={questId} 를 QuestTable에서 찾을 수 없습니다. ({evtType})");
            return;
        }

        if (quest.Quest_type != QuestType.Daily)
        {
            Debug.LogWarning($"[QuestManager] Quest_ID={questId} 는 Quest_Type={quest.Quest_type} 입니다. Daily가 아닙니다. ({evtType})");
        }

        CompleteDailyQuestInternal(quest);
    }

    /// <summary>
    /// 실제 Daily 퀘스트 완료 + 진행도 증가 + 세이브 저장
    /// </summary>
    private void CompleteDailyQuestInternal(QuestData quest)
    {
        var state = DailyState;

        int id = quest.Quest_ID;

        if (_completedDailyQuestIds.Contains(id))
        {
            // 이미 완료된 퀘스트
            return;
        }

        _completedDailyQuestIds.Add(id);

        if (!state.completedQuestIds.Contains(id))
            state.completedQuestIds.Add(id);

        // QuestData.Progress_Amount 만큼 진행도 증가 (0~100 clamp)
        int delta = quest.progress_amount;
        if (delta != 0)
        {
            int next = Mathf.Clamp(state.progress + delta, 0, 100);
            state.progress = next;
        }

        SaveLoadManager.SaveToServer().Forget();

        Debug.Log($"[QuestManager] Daily Quest 완료: id={id}, info={quest.Quest_info}, progress={state.progress}");
    }

    #endregion

    #region 추후 확장용 TODO (Weekly / Achievement)

    // 나중에 Weekly / Achievement 까지 확장할 때는:
    // - SaveDataV1에 Weekly용 / 업적용 상태 구조를 추가하고
    // - 아래처럼 QuestType 기준으로 그룹 상태를 나눠서 관리하면 된다.
    //
    // 예시의 스켈레톤만 남겨둔다. (지금은 컴파일 안 되게 주석 처리)

    /*
    private WeeklyQuestState WeeklyState => SaveLoadManager.Data.weeklyQuest;
    private AchievementQuestState AchievementState => SaveLoadManager.Data.achievementQuest;

    private void InitializeWeeklyQuests() { ... }
    private void InitializeAchievementQuests() { ... }

    public void OnMonsterKilledForAchievement(int monsterId) { ... }
    public void OnStageClearForWeekly(int stageId) { ... }
    */

    #endregion
}
