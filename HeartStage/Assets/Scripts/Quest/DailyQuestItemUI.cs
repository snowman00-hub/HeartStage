using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ScrollView 안에 생성되는 데일리 퀘스트 1개 UI
/// </summary>
public class DailyQuestItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI InfoText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button completeButton;
    [SerializeField] private GameObject completedMark; // 완료 체크 표시용 (없으면 비워둬도 됨)

    [Header("완료 상태 텍스트 & 색상")]
    [SerializeField] private TextMeshProUGUI stateText; // "미완료" / "완료" 표기용 (버튼 안 텍스트 등)
    [SerializeField] private Color normalButtonColor = Color.white;          // 기본 색
    [SerializeField] private Color completedButtonColor = new Color(0.7f, 0.7f, 0.7f); // 완료 후 약간 어두운 색

    [Header("아이콘 Addressables 키를 QuestData.Icon_image에서 읽음")]
    [SerializeField] private bool useIconAddressable = true;

    public int QuestId => questData != null ? questData.Quest_ID : 0;

    private DailyQuests owner;
    private QuestData questData;
    private bool isCompleted;

    public void Init(DailyQuests owner, QuestData data, bool completed)
    {
        this.owner = owner;
        this.questData = data;

        if (InfoText != null)
            InfoText.text = data.Quest_info; // CSV의 Quest_info를 제목/설명으로 사용

        if (useIconAddressable && iconImage != null && !string.IsNullOrEmpty(data.Icon_image))
        {
            LoadIconAsync(data.Icon_image);
        }

        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(OnClickComplete);
        }

        // 처음 생성될 때도 완료 상태 반영
        SetCompleted(completed);
    }

    private async void LoadIconAsync(string key)
    {
        try
        {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);
            var sprite = await handle.Task;
            if (iconImage != null)
                iconImage.sprite = sprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DailyQuestItemUI] 아이콘 로드 실패: {key}, {ex}");
        }
    }

    /// <summary>
    /// 퀘스트 완료/미완료 상태 UI 반영
    /// - 완료: stateText = "완료", 버튼 비활성, 색 어둡게
    /// - 미완료: stateText = "미완료", 버튼 활성, 색 기본
    /// </summary>
    public void SetCompleted(bool completed)
    {
        isCompleted = completed;

        if (completedMark != null)
            completedMark.SetActive(completed);

        if (completeButton != null)
        {
            completeButton.interactable = !completed;

            // 버튼 배경 색 변경 (Button의 targetGraphic을 기준으로)
            var targetGraphic = completeButton.targetGraphic as Image;
            if (targetGraphic != null)
            {
                targetGraphic.color = completed ? completedButtonColor : normalButtonColor;
            }
        }

        // 상태 텍스트 갱신
        if (stateText != null)
        {
            stateText.text = completed ? "완료" : "미완료";
        }
    }

    private void OnClickComplete()
    {
        if (isCompleted) return;
        if (owner == null || questData == null) return;

        owner.OnQuestItemClickedComplete(questData, this);
    }
}

