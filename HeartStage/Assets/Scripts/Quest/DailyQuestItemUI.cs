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

    [Header("아이콘 Addressables 키를 QuestData.Icon_Image에서 읽음")]
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
            InfoText.text = data.Quest_Info; // CSV의 Quest_Info를 제목/설명으로 사용

        if (useIconAddressable && iconImage != null && !string.IsNullOrEmpty(data.Icon_Image))
        {
            LoadIconAsync(data.Icon_Image);
        }

        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(OnClickComplete);
        }

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

    public void SetCompleted(bool completed)
    {
        isCompleted = completed;

        if (completedMark != null)
            completedMark.SetActive(completed);

        if (completeButton != null)
            completeButton.interactable = !completed;
    }

    private void OnClickComplete()
    {
        if (isCompleted) return;
        if (owner == null || questData == null) return;

        owner.OnQuestItemClickedComplete(questData, this);
    }
}

