using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProfileWindow : MonoBehaviour
{
    public static ProfileWindow Instance;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("상단 - 닉네임 / 칭호 / 팬 수")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Dropdown titleDropdown;
    [SerializeField] private TMP_Text fanCountText;

    [Header("아이콘 & 상태 메시지")]
    [SerializeField] private Image profileIconImage;
    [SerializeField] private TMP_Text statusMessageText;

    [Header("기록 박스 (메인 / 업적 / 팬미팅)")]
    [SerializeField] private TMP_Text mainStageText;
    [SerializeField] private TMP_Text achievementCountText;
    [SerializeField] private TMP_Text fanMeetingTimeText;
    // 스페셜 스테이지 타임은 아직 기획이 고정 안돼서 UI에서 제외
    // [SerializeField] private TMP_Text specialStageTimeText;

    // [Header("드림 에너지")]
    // [SerializeField] private TMP_Text dreamEnergyText;

    [Header("버튼들")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button changeNicknameButton;
    [SerializeField] private Button changeStatusButton;
    [SerializeField] private Button changeIconButton;

    [Header("변경창들")]
    [SerializeField] private NicknameWindow nicknameWindow;
    [SerializeField] private StatusMessageWindow statusMessageWindow;

    // 드랍다운 인덱스 → 타이틀 ID 매핑
    private readonly List<int> _titleIdByIndex = new List<int>();

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (changeNicknameButton != null && nicknameWindow != null)
            changeNicknameButton.onClick.AddListener(nicknameWindow.Open);

        if (changeStatusButton != null && statusMessageWindow != null)
            changeStatusButton.onClick.AddListener(statusMessageWindow.Open);

        if (changeIconButton != null)
            changeIconButton.onClick.AddListener(OnClickChangeIcon);

        if (titleDropdown != null)
        {
            titleDropdown.onValueChanged.AddListener(OnTitleDropdownChanged);
        }
    }

    private void OnEnable()
    {
        if (root != null && root.activeSelf)
        {
            RefreshAll();
        }
    }

    // ======= 외부에서 여는 함수 =======

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        RefreshAll();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }

    // ======= 메인 갱신 =======

    public void RefreshAll()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
        {
            Debug.LogWarning("[ProfileWindow] SaveDataV1 없음");
            return;
        }

        RefreshTopArea(data);
        RefreshIconAndStatus(data);
        RefreshRecordBox(data);
        // 드림 에너지 / 스페셜 스테이지 타임은 현재 프로필에서 사용하지 않음
        // RefreshDreamEnergy(data);
    }

    private void RefreshTopArea(SaveDataV1 data)
    {
        // 닉네임
        if (nicknameText != null)
        {
            string name = ProfileNameUtil.GetEffectiveNickname(data);
            nicknameText.text = name;
        }

        // 칭호 드랍다운
        RefreshTitleDropdown(data);

        // 팬 수
        if (fanCountText != null)
        {
            fanCountText.text = data.fanAmount.ToString("N0");
        }
    }

    private void RefreshTitleDropdown(SaveDataV1 data)
    {
        if (titleDropdown == null)
            return;

        _titleIdByIndex.Clear();
        titleDropdown.options.Clear();

        // 0번: 칭호 없음
        _titleIdByIndex.Add(0);
        titleDropdown.options.Add(new TMP_Dropdown.OptionData("칭호 없음"));

        // TitleTable에서 전체 리스트 가져오기
        var titleTable = DataTableManager.TitleTable; // 너 프로젝트 구조에 맞게 변경 가능
        Dictionary<int, TitleData> allTitles = null;
        if (titleTable != null)
            allTitles = titleTable.GetAll();

        // 내가 가진 칭호들만 돌면서 옵션 추가
        if (data.ownedTitleIds != null)
        {
            foreach (var titleId in data.ownedTitleIds)
            {
                TitleData tData = null;
                if (allTitles != null)
                    allTitles.TryGetValue(titleId, out tData);

                string displayName = tData != null ? tData.Title_name : $"Title {titleId}";
                _titleIdByIndex.Add(titleId);
                titleDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
            }
        }

        // 현재 장착된 칭호에 맞춰 드랍다운 선택 인덱스 설정
        int currentTitleId = data.equippedTitleId;
        int selectedIndex = 0; // 기본: 칭호 없음

        for (int i = 0; i < _titleIdByIndex.Count; i++)
        {
            if (_titleIdByIndex[i] == currentTitleId)
            {
                selectedIndex = i;
                break;
            }
        }

        titleDropdown.SetValueWithoutNotify(selectedIndex);
        titleDropdown.RefreshShownValue();
    }
    private void RefreshIconAndStatus(SaveDataV1 data)
    {
        Debug.Log("[ProfileWindow] RefreshIconAndStatus START");

        // 프로필 아이콘
        if (profileIconImage != null)
        {
            string key = ResolveProfileIconKey(data);
            Debug.Log($"[ProfileWindow] ResolveProfileIconKey => '{key}'");

            var sprite = string.IsNullOrEmpty(key)
                ? null
                : ResourceManager.Instance.Get<Sprite>(key);

            Debug.Log($"[ProfileWindow] Get<Sprite>('{key}') => {(sprite == null ? "NULL" : sprite.name)}");

            if (sprite != null)
            {
                profileIconImage.sprite = sprite;
                profileIconImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[ProfileWindow] 프로필 아이콘 로드 실패: {key}");
                profileIconImage.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("[ProfileWindow] profileIconImage == null");
        }

        // 상태 메시지
        if (statusMessageText != null)
        {
            if (string.IsNullOrEmpty(data.statusMessage))
                statusMessageText.text = "상태 메시지를 설정해 주세요.";
            else
                statusMessageText.text = data.statusMessage;
        }
    }

    private string ResolveProfileIconKey(SaveDataV1 data)
    {
        // 1) 이미 저장된 profileIconKey가 있고, 실제 스프라이트도 있으면 그대로 사용
        if (!string.IsNullOrEmpty(data.profileIconKey))
        {
            var cached = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
            if (cached != null)
                return data.profileIconKey;
        }

        // 2) 내가 가진 캐릭터(ownedIds) 중에서 아이콘을 하나 골라서 기본 아이콘으로 사용
        var charTable = DataTableManager.CharacterTable; // CSV 기반 캐릭터 테이블 :contentReference[oaicite:0]{index=0}
        var ownedIds = SaveLoadManager.Data.ownedIds;

        if (charTable != null && ownedIds != null && ownedIds.Count > 0)
        {
            foreach (var id in ownedIds)
            {
                var row = charTable.Get(id);
                if (row == null)
                    continue;

                // CSV에서 온 캐릭터 아이콘용 이름
                var iconKey = row.icon_imageName;
                if (string.IsNullOrEmpty(iconKey))
                    continue;

                // 실제로 Addressable로 로드돼 있는 Sprite인지 확인
                var sprite = ResourceManager.Instance.Get<Sprite>(iconKey);
                if (sprite == null)
                    continue;

                // 여기까지 왔으면 이 캐릭터 아이콘을 기본 프로필 아이콘으로 삼는다
                data.profileIconKey = iconKey;

                if (!data.ownedProfileIconKeys.Contains(iconKey))
                    data.ownedProfileIconKeys.Add(iconKey);

                // 세이브에 반영 (원하면 빼도 됨)
                SaveLoadManager.SaveToServer().Forget();

                return iconKey;
            }
        }

        // 3) 정말 캐릭터도 없고 아무것도 못 찾았을 때 최후의 기본값
        const string fallback = "hanaicon"; // 혹은 "ProfileIcon_Default"
        var fallbackSprite = ResourceManager.Instance.Get<Sprite>(fallback);
        if (fallbackSprite != null)
        {
            data.profileIconKey = fallback;
            SaveLoadManager.SaveToServer().Forget();
            return fallback;
        }

        // 4) 진짜로 아무것도 못 찾으면 빈 문자열
        return string.Empty;
    }

    private void RefreshRecordBox(SaveDataV1 data)
    {
        // 메인 스테이지
        if (mainStageText != null)
        {
            if (data.mainStageStep1 <= 0 && data.mainStageStep2 <= 0)
                mainStageText.text = "--";
            else
                mainStageText.text = $"{data.mainStageStep1}-{data.mainStageStep2}";
        }

        // 업적 개수
        if (achievementCountText != null)
        {
            int count = AchievementUtil.GetCompletedAchievementCount(data);
            achievementCountText.text = $"{count}개";
        }

        // 팬미팅 기록
        if (fanMeetingTimeText != null)
        {
            fanMeetingTimeText.text = FormatTimeMMSS(data.bestFanMeetingSeconds);
        }

        // 스페셜 스테이지 기록은 현재 UI에서 제거
        // if (specialStageTimeText != null)
        // {
        //     specialStageTimeText.text = FormatTimeMMSS(data.specialStageBestSeconds);
        // }
    }

    // private void RefreshDreamEnergy(SaveDataV1 data)
    // {
    //     if (dreamEnergyText != null)
    //     {
    //         dreamEnergyText.text = data.dreamEnergy.ToString("N0");
    //     }
    // }

    // ======= 버튼 콜백 =======

    private void OnClickChangeNickname()
    {
        if (NicknameWindow.Instance != null)
            NicknameWindow.Instance.Open();
    }

    private void OnClickChangeStatusMessage()
    {
        if (StatusMessageWindow.Instance != null)
            StatusMessageWindow.Instance.Open();
    }

    private void OnClickChangeIcon()
    {
        // TODO: 아이콘 선택창 열기
        Debug.Log("[ProfileWindow] 프로필 아이콘 변경 버튼 클릭");
    }

    // ======= 드랍다운 콜백 =======

    private void OnTitleDropdownChanged(int index)
    {
        ChangeTitleAsync(index).Forget();
    }

    private async UniTaskVoid ChangeTitleAsync(int index)
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        if (index < 0 || index >= _titleIdByIndex.Count)
            return;

        int newTitleId = _titleIdByIndex[index];

        // 값이 같으면 패스
        if (data.equippedTitleId == newTitleId)
            return;

        data.equippedTitleId = newTitleId;

        // 세이브 + publicProfiles 동기화
        await SaveLoadManager.SaveToServer();

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);
        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);
    }

    // ======= 유틸 =======

    private string FormatTimeMMSS(int seconds)
    {
        if (seconds <= 0)
            return "--:--";

        int m = seconds / 60;
        int s = seconds % 60;
        return $"{m:00}:{s:00}";
    }
}
