using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileWindow : MonoBehaviour
{
    public static ProfileWindow Instance;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("모달 패널")]
    [SerializeField] private ProfileModalPanel modalPanel;

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

    [Header("버튼들")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button changeNicknameButton;
    [SerializeField] private Button changeStatusButton;
    [SerializeField] private Button changeIconButton;

    [Header("팝업들")]
    [SerializeField] private NicknameWindow nicknameWindow;
    [SerializeField] private StatusMessageWindow statusMessageWindow;
    [SerializeField] private IconChangeWindow iconChangeWindow;

    // 드랍다운 인덱스 → 타이틀 ID 매핑
    private readonly List<int> _titleIdByIndex = new List<int>();

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (changeNicknameButton != null)
            changeNicknameButton.onClick.AddListener(OnClickChangeNickname);

        if (changeStatusButton != null)
            changeStatusButton.onClick.AddListener(OnClickChangeStatusMessage);

        if (changeIconButton != null)
            changeIconButton.onClick.AddListener(OnClickChangeIcon);

        if (titleDropdown != null)
            titleDropdown.onValueChanged.AddListener(OnTitleDropdownChanged);

        if (modalPanel != null)
            modalPanel.Hide();
    }

    private void OnEnable()
    {
        if (root != null && root.activeSelf)
        {
            RefreshAll();
        }
    }

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

        HideModalPanel();
        modalPanel?.CloseAllPopups();
    }

    public void ShowModalPanel()
    {
        modalPanel?.Show();
    }

    public void HideModalPanel()
    {
        modalPanel?.Hide();
    }

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
            fanCountText.text = $"♥ 팬: {data.fanAmount}";
        }
    }

    private void RefreshTitleDropdown(SaveDataV1 data)
    {
        if (titleDropdown == null)
            return;

        _titleIdByIndex.Clear();
        titleDropdown.options.Clear();

        _titleIdByIndex.Add(0);
        titleDropdown.options.Add(new TMP_Dropdown.OptionData("칭호 없음"));

        var titleTable = DataTableManager.TitleTable;
        Dictionary<int, TitleData> allTitles = null;
        if (titleTable != null)
            allTitles = titleTable.GetAll();

        if (data.ownedTitleIds != null)
        {
            foreach (var titleId in data.ownedTitleIds)
            {
                TitleData tData = null;
                allTitles?.TryGetValue(titleId, out tData);

                string displayName = tData != null ? tData.Title_name : $"Title {titleId}";
                _titleIdByIndex.Add(titleId);
                titleDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
            }
        }

        int currentTitleId = data.equippedTitleId;
        int selectedIndex = 0;

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
        // 1) 이미 저장된 profileIconKey가 있고 실제 스프라이트도 있으면 그대로 사용
        if (!string.IsNullOrEmpty(data.profileIconKey))
        {
            var cached = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
            if (cached != null)
                return data.profileIconKey;
        }

        var charTable = DataTableManager.CharacterTable;
        var unlocked = data.unlockedByName;

        if (charTable != null && unlocked != null && unlocked.Count > 0)
        {
            string.Join(", ", unlocked.Where(p => p.Value).Select(p => p.Key));

            foreach (var kv in unlocked)
            {
                string charName = kv.Key;
                bool isUnlocked = kv.Value;

                if (!isUnlocked)
                    continue;

                var row = charTable.GetByName(charName);

                if (row == null)
                    continue;

                string iconKey = row.icon_imageName;

                if (string.IsNullOrEmpty(iconKey))
                    continue;

                var sprite = ResourceManager.Instance.GetSprite(iconKey);

                if (sprite == null)
                    continue;

                // 여기 오면 진짜 성공
                data.profileIconKey = iconKey;
                
        return iconKey;
            }
        }

        // 3) unlockedByName에서도 못 찾았으면 최후의 fallback
        const string fallback = "hanaicon";

        var fallbackSprite = ResourceManager.Instance.GetSprite("hanaicon");
        if (fallbackSprite != null)
        {
            data.profileIconKey = fallback;
            SaveLoadManager.SaveToServer().Forget();
            return fallback;
        }

        // 4) 진짜 아무것도 없으면 빈 문자열
        return string.Empty;
    }


    private void RefreshRecordBox(SaveDataV1 data)
    {
        if (mainStageText != null)
        {
            if (data.mainStageStep1 <= 0 && data.mainStageStep2 <= 0)
                mainStageText.text = "메인 스테이지 진행도: 없음";
            else
                mainStageText.text = $"메인 스테이지 진행도: {data.mainStageStep1}-{data.mainStageStep2}";
        }

        if (achievementCountText != null)
        {
            int count = AchievementUtil.GetCompletedAchievementCount(data);
            achievementCountText.text = $"달성한 업적: {count}개";
        }

        if (fanMeetingTimeText != null)
        {
            fanMeetingTimeText.text = FormatTimeMMSS(data.bestFanMeetingSeconds);
        }
    }

    private void OnClickChangeNickname()
    {
        if (nicknameWindow != null)
        {
            nicknameWindow.Open();
            ShowModalPanel();
        }
    }

    private void OnClickChangeStatusMessage()
    {
        if (statusMessageWindow != null)
        {
            statusMessageWindow.Open();
            ShowModalPanel();
        }
    }

    private void OnClickChangeIcon()
    {
        if (iconChangeWindow != null)
        {
            iconChangeWindow.Open();
            ShowModalPanel();
        }
    }

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

        if (data.equippedTitleId == newTitleId)
            return;

        data.equippedTitleId = newTitleId;

        await SaveLoadManager.SaveToServer();

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);
        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);
    }

    private string FormatTimeMMSS(int seconds)
    {
        if (seconds <= 0)
            return "팬미팅 진행시간: 없음";

        int m = seconds / 60;
        int s = seconds % 60;
        return $"팬미팅 진행시간: {m:00}:{s:00}";
    }
}
