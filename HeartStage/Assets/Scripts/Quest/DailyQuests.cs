using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Daily Quest 진행도 게이지 + 5개 보상 버튼 UI + Firebase 연동
/// 
/// - 진행도 게이지는 외부에서 SetProgress / AddProgress 로만 변경
/// - 20, 40, 60, 80, 100 구간에서 버튼 활성화
/// - 버튼 상태별 아이콘(잠김/수령 가능/수령 완료)은 QuestProgressTable의
///   NotFill_Icon / Filled_Icon / Get_Reward_Icon 문자열을 Addressables 키로 사용
/// - Firebase Realtime DB에 오늘 진행도 및 각 버튼 수령 여부 저장
/// </summary>
public class DailyQuests : MonoBehaviour
{
    #region 내부 클래스

    [Serializable]
    private class RewardButtonSlot
    {
        [Header("UI 참조")]
        public Button button;
        public Image iconImage;          // 버튼 안의 아이콘 Image

        [Header("퀘스트 진척도 ID (QuestProgressTable.csv의 progress_reward_ID)")]
        public int progressRewardId;     // 예: 13201020, 13201040, ...

        [NonSerialized] public QuestProgressData data;
        [NonSerialized] public bool isClaimed;

        // 아이콘 스프라이트 (Addressables 로드 후 캐싱)
        [NonSerialized] public Sprite notFilledSprite;
        [NonSerialized] public Sprite filledSprite;
        [NonSerialized] public Sprite claimedSprite;
    }

    [Serializable]
    private class DailyProgressState
    {
        public int progress;
        public bool[] claimed;  // length = rewardSlots.Length
    }

    #endregion

    [Header("진행도 게이지")]
    [SerializeField] private Slider progressSlider;

    [Header("진행도 보상 버튼 슬롯들 (5개)")]
    [SerializeField] private RewardButtonSlot[] rewardSlots = new RewardButtonSlot[5];

    [Header("테이블 참조")]
    [SerializeField] private QuestProgressTable questProgressTable;

    [Header("이 진행도는 어떤 타입인가? (ProgressType.Daily 추천)")]
    [SerializeField] private ProgressType progressType = ProgressType.Daily;

    [Header("Firebase 경로 설정")]
    [SerializeField] private string firebaseDailyPathFormat = "users/{0}/daily/{1}";
    // {0} = uid, {1} = yyyyMMdd 날짜키

    // Firebase 관련
    private FirebaseApp _firebaseApp;
    private FirebaseAuth _auth;
    private DatabaseReference _dbRef;
    private string _uid;
    private string _todayKey;

    private bool _firebaseReady = false;
    private bool _loadedFromFirebase = false;

    // 내부 진행도 캐시 (0~100)
    public int CurrentProgress { get; private set; }

    #region Unity Lifecycle

    private async void Start()
    {
        // 슬라이더 기본 세팅
        if (progressSlider != null)
        {
            if (progressSlider.minValue != 0) progressSlider.minValue = 0;
            if (progressSlider.maxValue != 100) progressSlider.maxValue = 100;
            progressSlider.wholeNumbers = true;
        }

        // 버튼 클릭 이벤트 등록
        HookButtonEvents();

        // 테이블에서 ProgressData 가져오기
        InitQuestProgressDataFromTable();

        // 아이콘 Addressables 로드
        await LoadRewardIconsAsync();

        // Firebase 초기화 + 오늘 데이터 로딩
        await InitFirebaseAndLoadStateAsync();

        // 슬라이더/버튼 상태 갱신
        ApplyStateToUI();
    }

    #endregion

    #region 초기화 관련

    private void HookButtonEvents()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            int index = i; // 클로저 캡쳐용
            if (rewardSlots[i]?.button != null)
            {
                rewardSlots[i].button.onClick.RemoveAllListeners();
                rewardSlots[i].button.onClick.AddListener(() => OnClickRewardButton(index));
            }
        }
    }

    /// <summary>
    /// QuestProgressTable 에서 각 progressRewardId 에 맞는 데이터 찾아서 채워넣기
    /// </summary>
    private void InitQuestProgressDataFromTable()
    {
        if (questProgressTable == null)
        {
            Debug.LogError("[DailyQuests] QuestProgressTable 참조가 없습니다. 인스펙터에서 연결해 주세요.");
            return;
        }

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null) continue;

            if (slot.progressRewardId == 0)
            {
                Debug.LogWarning($"[DailyQuests] 슬롯 {i} 의 progressRewardId 가 0 입니다. CSV의 progress_reward_ID 를 넣어주세요.");
                continue;
            }

            var data = questProgressTable.Get(slot.progressRewardId);
            if (data == null)
            {
                Debug.LogError($"[DailyQuests] QuestProgressData 를 찾을 수 없습니다. ID: {slot.progressRewardId}");
                continue;
            }

            // 타입 체크 (예: Daily 만 사용하고 싶을 때)
            if ((ProgressType)data.Progress_Type != progressType)
            {
                Debug.LogWarning($"[DailyQuests] 슬롯 {i} 의 Progress_Type 이 현재 UI에서 사용 중인 타입({progressType})과 다릅니다. CSV 확인 필요. ID: {slot.progressRewardId}");
            }

            slot.data = data;
        }
    }

    /// <summary>
    /// QuestProgressData 에 적힌 아이콘 문자열을 Addressables 키로 사용해 스프라이트 로드
    /// </summary>
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
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(slot.data.NotFill_Icon);
                    slot.notFilledSprite = await handle.Task;
                }

                // Filled_Icon
                if (!string.IsNullOrEmpty(slot.data.Filled_Icon))
                {
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(slot.data.Filled_Icon);
                    slot.filledSprite = await handle.Task;
                }

                // Get_Reward_Icon
                if (!string.IsNullOrEmpty(slot.data.Get_Reward_Icon))
                {
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(slot.data.Get_Reward_Icon);
                    slot.claimedSprite = await handle.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DailyQuests] 아이콘 로드 실패 (slot {i}, id {slot.progressRewardId}) : {ex}");
            }
        }
    }

    /// <summary>
    /// Firebase 초기화 + 오늘 날짜 기준 DailyProgressState 로딩
    /// </summary>
    private async Task InitFirebaseAndLoadStateAsync()
    {
        // 오늘 날짜 키 (KST 기준으로 하고 싶으면 TimeZone 보정해서 써도 됨)
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
            // 아직 로그인 안 되어있으면 익명 로그인
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

        // 4) 오늘 상태 로딩
        await LoadStateFromFirebaseAsync();
    }

    private async Task LoadStateFromFirebaseAsync()
    {
        if (!_firebaseReady || _dbRef == null || string.IsNullOrEmpty(_uid))
        {
            Debug.LogWarning("[DailyQuests] Firebase 준비가 안 되어 있어 로딩을 생략합니다.");
            return;
        }

        string path = string.Format(firebaseDailyPathFormat, _uid, _todayKey);
        var getTask = _dbRef.Child(path).GetValueAsync();
        await getTask;

        if (getTask.Exception != null)
        {
            Debug.LogError($"[DailyQuests] Firebase 데이터 로딩 실패: {getTask.Exception}");
            return;
        }

        DataSnapshot snapshot = getTask.Result;
        if (!snapshot.Exists)
        {
            // 오늘 첫 진입: 기본값 상태
            CurrentProgress = 0;
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                if (rewardSlots[i] != null)
                    rewardSlots[i].isClaimed = false;
            }
        }
        else
        {
            string json = snapshot.GetRawJsonValue();
            if (!string.IsNullOrEmpty(json))
            {
                var state = JsonUtility.FromJson<DailyProgressState>(json);
                if (state != null)
                {
                    CurrentProgress = state.progress;

                    // 길이 맞춰서 수령 여부 복구
                    for (int i = 0; i < rewardSlots.Length; i++)
                    {
                        bool claimed = false;
                        if (state.claimed != null && i < state.claimed.Length)
                            claimed = state.claimed[i];

                        if (rewardSlots[i] != null)
                            rewardSlots[i].isClaimed = claimed;
                    }
                }
            }
        }

        _loadedFromFirebase = true;
    }

    private async Task SaveStateToFirebaseAsync()
    {
        if (!_firebaseReady || _dbRef == null || string.IsNullOrEmpty(_uid))
            return;

        var state = new DailyProgressState
        {
            progress = CurrentProgress,
            claimed = new bool[rewardSlots.Length]
        };

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            state.claimed[i] = rewardSlots[i] != null && rewardSlots[i].isClaimed;
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

    #region 진행도 / 버튼 로직

    /// <summary>
    /// 외부에서 진행도를 절대값으로 세팅 (0~100)
    /// Daily Quest 완료 시 여기로 호출해주면 됨.
    /// </summary>
    public async void SetProgress(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        CurrentProgress = value;
        if (progressSlider != null)
            progressSlider.value = CurrentProgress;

        UpdateRewardButtons();

        // Firebase 저장
        await SaveStateToFirebaseAsync();
    }

    /// <summary>
    /// 외부에서 진행도 추가 (누적)
    /// 예: Daily Quest 1개 완료 시, 그 퀘스트의 progress_amount 만큼 더해줌.
    /// </summary>
    public void AddProgress(int delta)
    {
        int next = Mathf.Clamp(CurrentProgress + delta, 0, 100);
        SetProgress(next);
    }

    /// <summary>
    /// 오늘 상태를 UI에 반영
    /// </summary>
    private void ApplyStateToUI()
    {
        if (progressSlider != null)
            progressSlider.value = CurrentProgress;

        UpdateRewardButtons();
    }

    /// <summary>
    /// 진행도 & 수령 여부에 따라 버튼 interacable + 아이콘 변경
    /// </summary>
    private void UpdateRewardButtons()
    {
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            var slot = rewardSlots[i];
            if (slot == null || slot.button == null || slot.iconImage == null || slot.data == null)
                continue;

            int requireProgress = slot.data.Progress_Amount; // CSV의 progress_amount (20,40,60,...)

            if (slot.isClaimed)
            {
                // 이미 수령 완료
                slot.button.interactable = false;
                if (slot.claimedSprite != null)
                    slot.iconImage.sprite = slot.claimedSprite;
            }
            else
            {
                // 아직 수령 안함
                if (CurrentProgress >= requireProgress)
                {
                    // 수령 가능
                    slot.button.interactable = true;
                    if (slot.filledSprite != null)
                        slot.iconImage.sprite = slot.filledSprite;
                }
                else
                {
                    // 조건 미달
                    slot.button.interactable = false;
                    if (slot.notFilledSprite != null)
                        slot.iconImage.sprite = slot.notFilledSprite;
                }
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 보상 지급 + 수령 처리
    /// </summary>
    private async void OnClickRewardButton(int index)
    {
        if (index < 0 || index >= rewardSlots.Length) return;
        var slot = rewardSlots[index];
        if (slot == null || slot.data == null) return;
        if (slot.isClaimed) return; // 이미 받은 보상

        int requireProgress = slot.data.Progress_Amount;
        if (CurrentProgress < requireProgress)
        {
            Debug.Log("[DailyQuests] 조건이 충족되지 않았습니다.");
            return;
        }

        // 1) 실제 보상 지급 (ItemManager 등과 연동)
        GiveProgressReward(slot.data);

        // 2) 수령 완료 표시
        slot.isClaimed = true;
        UpdateRewardButtons();

        // 3) Firebase 저장
        await SaveStateToFirebaseAsync();
    }

    /// <summary>
    /// QuestProgressData 에 적힌 reward1~3을 실제 인게임 아이템/재화로 지급하는 부분
    /// </summary>
    private void GiveProgressReward(QuestProgressData data)
    {
        // 예시: ItemManager를 사용한다고 가정
        // 실제 프로젝트 구조에 맞게 이 부분 교체하면 됨.

        // reward1
        if (data.Reward1 != 0 && data.Reward1_Amount > 0)
        {
            // ItemManager.Instance.AddItem(data.Reward1, data.Reward1_Amount);
            Debug.Log($"[DailyQuests] 보상1 지급: ID={data.Reward1}, Amount={data.Reward1_Amount}");
        }

        // reward2
        if (data.Reward2 != 0 && data.Reward2_Amount > 0)
        {
            // ItemManager.Instance.AddItem(data.Reward2, data.Reward2_Amount);
            Debug.Log($"[DailyQuests] 보상2 지급: ID={data.Reward2}, Amount={data.Reward2_Amount}");
        }

        // reward3
        if (data.Reward3 != 0 && data.Reward3_Amount > 0)
        {
            // ItemManager.Instance.AddItem(data.Reward3, data.Reward3_Amount);
            Debug.Log($"[DailyQuests] 보상3 지급: ID={data.Reward3}, Amount={data.Reward3_Amount}");
        }
    }

    #endregion

    #region 외부에서 Daily Quest 완료 시 호출 예시

    /// <summary>
    /// 다른 곳(QuestManager 등)에서 Daily Quest를 완료했을 때 불러주면 좋은 헬퍼 함수.
    /// 
    /// - Daily 가 아닌 퀘스트 타입은 무시
    /// - 같은 퀘스트를 여러 번 완료해도 AddProgress를 또 할지, 하루 1회만 인정할지는
    ///   Firebase에 "오늘 완료한 daily quest id 리스트" 저장해서 체크하면 됨.
    ///   (그 부분은 여기서는 단순화를 위해 생략)
    /// </summary>
    public void OnDailyQuestCompleted(QuestData questData)
    {
        if (questData == null) return;
        if (questData.Quest_Type != QuestType.Daily) return;

        // 이 퀘스트가 주는 진행도 만큼 더해줌 (QuestTable.csv의 progress_amount)
        int amount = questData.Progress_Amount;
        if (amount <= 0) return;

        AddProgress(amount);
    }

    #endregion
}

