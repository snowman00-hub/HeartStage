using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DailyQuests : MonoBehaviour
{
    #region 내부 클래스들

    [Serializable]
    private class RewardButtonSlot
    {
        [Header("UI")]
        public Button button;
        public Image iconImage;          // 버튼 안의 아이콘 Image

        [Header("QuestProgressTable.csv 의 progress_reward_ID")]
        public int progressRewardId;     // 예: 13201020, 13201040, ...

        [NonSerialized] public QuestProgressData data;
        [NonSerialized] public bool isClaimed;

        // Addressables 로드 후 캐싱
        [NonSerialized] public Sprite notFilledSprite;
        [NonSerialized] public Sprite filledSprite;
        [NonSerialized] public Sprite claimedSprite;
    }

    [Serializable]
    private class DailyProgressState
    {
        public int progress;          // 오늘 진행도 (0~100)
        public bool[] claimed;        // 보상 5개 수령 여부
        public int[] completedQuestIds; // 오늘 완료한 데일리 퀘스트 ID 목록
    }

    #endregion

    [Header("진행도 게이지")]
    [SerializeField] private Slider progressSlider;

    [Header("진행도 보상 버튼 슬롯들 (5개)")]
    [SerializeField] private RewardButtonSlot[] rewardSlots = new RewardButtonSlot[5];

    [Header("데일리 퀘스트 리스트 (ScrollView Content)")]
    [SerializeField] private Transform questListContent;     // ScrollView 의 Content
    [SerializeField] private DailyQuestItemUI questItemPrefab; // 프리팹
    [SerializeField] private int[] dailyQuestIds;            // 오늘 사용할 데일리 퀘스트 ID들

    [Header("테이블 참조")]
    [SerializeField] private QuestProgressTable questProgressTable;
    [SerializeField] private QuestTable questTable;

    [Header("이 진행도는 어떤 타입인가?")]
    [SerializeField] private ProgressType progressType = ProgressType.Daily; // 1: Daily

    [Header("Firebase 경로 설정")]
    [SerializeField] private string firebaseDailyPathFormat = "users/{0}/daily/{1}";
    // {0}=uid, {1}=yyyyMMdd

    // Firebase
    private FirebaseApp _firebaseApp;
    private FirebaseAuth _auth;
    private DatabaseReference _dbRef;
    private string _uid;
    private string _todayKey;
    private bool _firebaseReady = false;

    // 상태 캐시
    public int CurrentProgress { get; private set; }
    private readonly HashSet<int> _completedDailyQuestIds = new HashSet<int>(); // 오늘 완료한 데일리 퀘스트 ID
    private readonly List<DailyQuestItemUI> _questItems = new List<DailyQuestItemUI>();

    #region Unity Lifecycle

    private async void Start()
    {
        // 슬라이더 기본 세팅
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = 100;
            progressSlider.wholeNumbers = true;
        }

        // 버튼 클릭 이벤트 연결
        HookButtonEvents();

        // QuestProgressTable 에서 각각의 보상 데이터 세팅
        InitQuestProgressDataFromTable();

        // 보상 버튼 아이콘 Addressables 로드
        await LoadRewardIconsAsync();

        // Firebase 초기화 + 오늘 상태 로딩
        await InitFirebaseAndLoadStateAsync();

        // UI 반영
        ApplyStateToUI();

        // 데일리 퀘스트 리스트 생성 (ScrollView Content 아래에)
        CreateDailyQuestItems();
    }

    #endregion

    #region 초기화 (보상 버튼)

    private void HookButtonEvents()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            int index = i;
            if (rewardSlots[i]?.button != null)
            {
                rewardSlots[i].button.onClick.RemoveAllListeners();
                rewardSlots[i].button.onClick.AddListener(() => OnClickRewardButton(index));
            }
        }
    }

    private void InitQuestProgressDataFromTable()
    {
        if (questProgressTable == null)
        {
            Debug.LogError("[DailyQuests] QuestProgressTable 참조가 없습니다.");
            return;
        }

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null) continue;

            if (slot.progressRewardId == 0)
            {
                Debug.LogWarning($"[DailyQuests] 슬롯 {i} 의 progressRewardId 가 0 입니다.");
                continue;
            }

            var data = questProgressTable.Get(slot.progressRewardId);
            if (data == null)
            {
                Debug.LogError($"[DailyQuests] QuestProgressData 를 찾을 수 없습니다. ID: {slot.progressRewardId}");
                continue;
            }

            // 타입 체크 (Daily 만 쓰고 싶다면)
            if ((ProgressType)data.Progress_Type != progressType)
            {
                Debug.LogWarning($"[DailyQuests] 슬롯 {i} 의 Progress_Type 이 현재 UI 타입({progressType})과 다릅니다. ID:{slot.progressRewardId}");
            }

            slot.data = data;
        }
    }

    private async Task LoadRewardIconsAsync()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null || slot.data == null || slot.iconImage == null)
                continue;

            try
            {
                // NotFill_Icon
                if (!string.IsNullOrEmpty(slot.data.NotFill_Icon))
                {
                    AsyncOperationHandle<Sprite> h = Addressables.LoadAssetAsync<Sprite>(slot.data.NotFill_Icon);
                    slot.notFilledSprite = await h.Task;
                }

                // Filled_Icon
                if (!string.IsNullOrEmpty(slot.data.Filled_Icon))
                {
                    AsyncOperationHandle<Sprite> h = Addressables.LoadAssetAsync<Sprite>(slot.data.Filled_Icon);
                    slot.filledSprite = await h.Task;
                }

                // Get_Reward_Icon
                if (!string.IsNullOrEmpty(slot.data.Get_Reward_Icon))
                {
                    AsyncOperationHandle<Sprite> h = Addressables.LoadAssetAsync<Sprite>(slot.data.Get_Reward_Icon);
                    slot.claimedSprite = await h.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DailyQuests] 아이콘 로드 실패 (slot {i}, id {slot.progressRewardId}) : {ex}");
            }
        }
    }

    #endregion

    #region Firebase 로딩/저장

    private async Task InitFirebaseAndLoadStateAsync()
    {
        _todayKey = DateTime.UtcNow.ToLocalTime().ToString("yyyyMMdd");

        // 1) Firebase 의존성 체크
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"[DailyQuests] Firebase 의존성 에러: {dependencyStatus}");
            _firebaseReady = false;
            return;
        }

        _firebaseApp = FirebaseApp.DefaultInstance;

        // 2) Auth
        _auth = FirebaseAuth.DefaultInstance;
        if (_auth.CurrentUser == null)
        {
            var signInTask = _auth.SignInAnonymouslyAsync();
            await signInTask;

            if (signInTask.Exception != null)
            {
                Debug.LogError($"[DailyQuests] 익명 로그인 실패: {signInTask.Exception}");
                _firebaseReady = false;
                return;
            }
        }

        _uid = _auth.CurrentUser.UserId;

        // 3) Database
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        _firebaseReady = true;

        // 4) 오늘 데이터 로드
        await LoadStateFromFirebaseAsync();
    }

    private async Task LoadStateFromFirebaseAsync()
    {
        if (!_firebaseReady || _dbRef == null || string.IsNullOrEmpty(_uid))
            return;

        string path = string.Format(firebaseDailyPathFormat, _uid, _todayKey);
        var getTask = _dbRef.Child(path).GetValueAsync();
        await getTask;

        if (getTask.Exception != null)
        {
            Debug.LogError($"[DailyQuests] Firebase 데이터 로딩 실패: {getTask.Exception}");
            return;
        }

        DataSnapshot snapshot = getTask.Result;
        _completedDailyQuestIds.Clear();

        if (!snapshot.Exists)
        {
            // 오늘 첫 접속
            CurrentProgress = 0;
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                if (rewardSlots[i] != null)
                    rewardSlots[i].isClaimed = false;
            }
            return;
        }

        string json = snapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json))
            return;

        var state = JsonUtility.FromJson<DailyProgressState>(json);
        if (state == null)
            return;

        CurrentProgress = state.progress;

        // 보상 수령 여부
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            bool claimed = false;
            if (state.claimed != null && i < state.claimed.Length)
                claimed = state.claimed[i];

            if (rewardSlots[i] != null)
                rewardSlots[i].isClaimed = claimed;
        }

        // 완료한 데일리 퀘스트 ID 세트 생성
        if (state.completedQuestIds != null)
        {
            foreach (int id in state.completedQuestIds)
                _completedDailyQuestIds.Add(id);
        }
    }

    private async Task SaveStateToFirebaseAsync()
    {
        if (!_firebaseReady || _dbRef == null || string.IsNullOrEmpty(_uid))
            return;

        var state = new DailyProgressState
        {
            progress = CurrentProgress,
            claimed = new bool[rewardSlots.Length],
            completedQuestIds = new int[_completedDailyQuestIds.Count]
        };

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            state.claimed[i] = rewardSlots[i] != null && rewardSlots[i].isClaimed;
        }

        int idx = 0;
        foreach (int id in _completedDailyQuestIds)
        {
            state.completedQuestIds[idx++] = id;
        }

        string json = JsonUtility.ToJson(state);
        string path = string.Format(firebaseDailyPathFormat, _uid, _todayKey);

        var setTask = _dbRef.Child(path).SetRawJsonValueAsync(json);
        await setTask;

        if (setTask.Exception != null)
        {
            Debug.LogError($"[DailyQuests] Firebase 저장 실패: {setTask.Exception}");
        }
    }

    #endregion

    #region 진행도 / 보상 버튼 로직

    public async void SetProgress(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        CurrentProgress = value;
        if (progressSlider != null)
            progressSlider.value = CurrentProgress;

        UpdateRewardButtons();
        await SaveStateToFirebaseAsync();
    }

    public void AddProgress(int delta)
    {
        int next = Mathf.Clamp(CurrentProgress + delta, 0, 100);
        SetProgress(next);
    }

    private void ApplyStateToUI()
    {
        if (progressSlider != null)
            progressSlider.value = CurrentProgress;

        UpdateRewardButtons();
    }

    private void UpdateRewardButtons()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null || slot.button == null || slot.iconImage == null || slot.data == null)
                continue;

            int requireProgress = slot.data.Progress_Amount; // 20,40,60,80,100

            if (slot.isClaimed)
            {
                slot.button.interactable = false;
                if (slot.claimedSprite != null)
                    slot.iconImage.sprite = slot.claimedSprite;
            }
            else
            {
                if (CurrentProgress >= requireProgress)
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

    private async void OnClickRewardButton(int index)
    {
        if (index < 0 || index >= rewardSlots.Length) return;
        var slot = rewardSlots[index];
        if (slot == null || slot.data == null) return;
        if (slot.isClaimed) return;

        int requireProgress = slot.data.Progress_Amount;
        if (CurrentProgress < requireProgress)
        {
            Debug.Log("[DailyQuests] 보상 조건 미충족");
            return;
        }

        // 실제 보상 지급
        GiveProgressReward(slot.data);

        slot.isClaimed = true;
        UpdateRewardButtons();
        await SaveStateToFirebaseAsync();
    }

    private void GiveProgressReward(QuestProgressData data)
    {
        // TODO: 실제 인게임 ItemManager 등과 연동
        if (data.Reward1 != 0 && data.Reward1_Amount > 0)
            Debug.Log($"[DailyQuests] Reward1: {data.Reward1} x {data.Reward1_Amount}");
        if (data.Reward2 != 0 && data.Reward2_Amount > 0)
            Debug.Log($"[DailyQuests] Reward2: {data.Reward2} x {data.Reward2_Amount}");
        if (data.Reward3 != 0 && data.Reward3_Amount > 0)
            Debug.Log($"[DailyQuests] Reward3: {data.Reward3} x {data.Reward3_Amount}");
    }

    #endregion

    #region 스크롤뷰 안에 Daily Quest 생성

    private void CreateDailyQuestItems()
    {
        if (questListContent == null || questItemPrefab == null || questTable == null)
        {
            Debug.LogWarning("[DailyQuests] Quest 리스트 생성에 필요한 참조가 없습니다.");
            return;
        }

        // 기존에 남아있던 것 정리
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        _questItems.Clear();

        if (dailyQuestIds == null || dailyQuestIds.Length == 0)
        {
            Debug.LogWarning("[DailyQuests] dailyQuestIds 가 비어있습니다. 오늘 사용할 데일리 퀘스트 ID를 넣어주세요.");
            return;
        }

        foreach (int id in dailyQuestIds)
        {
            var data = questTable.Get(id);
            if (data == null)
            {
                Debug.LogWarning($"[DailyQuests] QuestTable 에서 데일리 퀘스트를 찾지 못했습니다. ID: {id}");
                continue;
            }

            if (data.Quest_Type != QuestType.Daily)
            {
                Debug.LogWarning($"[DailyQuests] Quest_ID {id} 가 Daily 타입이 아닙니다. CSV 확인 필요.");
            }

            bool completed = _completedDailyQuestIds.Contains(data.Quest_ID);

            var item = Instantiate(questItemPrefab, questListContent);
            item.Init(this, data, completed);

            _questItems.Add(item);
        }
    }

    /// <summary>
    /// 퀘스트 아이템에서 "완료/보상 받기" 버튼 눌렀을 때 DailyQuests로 콜백
    /// </summary>
    public async void OnQuestItemClickedComplete(QuestData questData, DailyQuestItemUI itemUI)
    {
        if (questData == null || itemUI == null) return;

        int questId = questData.Quest_ID;
        if (_completedDailyQuestIds.Contains(questId))
            return; // 이미 완료 처리됨

        _completedDailyQuestIds.Add(questId);
        itemUI.SetCompleted(true);

        // 이 퀘스트가 데일리 진행도에 영향을 주면 진행도 증가
        if (questData.Progress_Type == (int)progressType &&
            questData.Progress_Amount > 0)
        {
            AddProgress(questData.Progress_Amount);
        }
        else
        {
            // 진행도는 안 올리지만 완료 정보는 저장
            await SaveStateToFirebaseAsync();
        }
    }

    #endregion

    #region 외부에서 퀘스트 완료 알리고 싶은 경우

    /// <summary>
    /// 다른 매니저(QuestManager 등)에서 데일리 퀘스트 완료를 알려주고 싶을 때 사용.
    /// ScrollView 버튼 말고도, 게임 안 이벤트로 자동 완료 처리 가능.
    /// </summary>
    public void OnDailyQuestCompletedExternally(QuestData questData)
    {
        if (questData == null) return;
        if (questData.Quest_Type != QuestType.Daily) return;

        // 이미 완료된 퀘스트면 무시
        if (_completedDailyQuestIds.Contains(questData.Quest_ID))
            return;

        // _questItems 중 같은 ID 찾아서 UI도 같이 갱신해줌
        DailyQuestItemUI ui = _questItems.Find(x => x.QuestId == questData.Quest_ID);
        if (ui != null)
        {
            OnQuestItemClickedComplete(questData, ui);
        }
        else
        {
            // 리스트에 없는 퀘스트라면 그냥 진행도만 올리기
            if (questData.Progress_Type == (int)progressType &&
                questData.Progress_Amount > 0)
            {
                AddProgress(questData.Progress_Amount);
            }
        }
    }

    #endregion
}


