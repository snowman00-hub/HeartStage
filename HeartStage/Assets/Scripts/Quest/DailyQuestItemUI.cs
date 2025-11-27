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

        // ★ 아직 QuestManager에서 완료 처리 안 된 퀘스트면 그냥 막아버림
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[DailyQuestItemUI] QuestManager.Instance 가 없습니다.");
            return;
        }

        // QuestManager 기준으로 아직 클리어 안 되었으면 UI에서 강제 완료 금지
        if (!QuestManager.Instance.IsDailyQuestCompleted(questData.Quest_ID))
        {
            Debug.Log("[DailyQuestItemUI] 아직 클리어 조건을 만족하지 않은 퀘스트입니다.");
            // TODO: 여기서 토스트 팝업 / 안내창 띄우면 됨.
            return;
        }

        // 여기까지 왔다는 건 정말로 완료된 퀘스트 → UI에서 '완료 처리'만 한다.
        owner.OnQuestItemClickedComplete(questData, this);
    }
}

