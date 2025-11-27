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

    private bool isCleared;   // 조건 충족 여부
    private bool isCompleted; // 보상 수령 여부

    public void Init(DailyQuests owner, QuestData data, bool cleared, bool completed)
    {
        this.owner = owner;
        this.questData = data;

        if (InfoText != null)
            InfoText.text = data.Quest_info;

        if (useIconAddressable && iconImage != null && !string.IsNullOrEmpty(data.Icon_image))
        {
            LoadIconAsync(data.Icon_image);
        }

        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(OnClickComplete);
        }

        SetState(cleared, completed);
    }

    /// 퀘스트 상태 UI 반영
    /// - cleared=false, completed=false : 미완료 (조건 미충족)
    /// - cleared=true,  completed=false : 완료 (보상 수령 가능, 버튼 활성)
    /// - cleared=true,  completed=true  : 완료 (보상 수령 완료, 버튼 비활성 + 체크)
    public void SetState(bool cleared, bool completed)
    {
        isCleared = cleared;
        isCompleted = completed;

        if (completedMark != null)
            completedMark.SetActive(completed);

        if (completeButton != null)
        {
            if (!cleared)
            {
                completeButton.interactable = false;
            }
            else
            {
                // 완료 상태든 수령 전이든, 조건만 충족되면 버튼은 한 번은 눌러볼 수 있음
                completeButton.interactable = !completed;
            }

            var targetGraphic = completeButton.targetGraphic as Image;
            if (targetGraphic != null)
            {
                if (completed)
                    targetGraphic.color = completedButtonColor;
                else
                    targetGraphic.color = normalButtonColor;
            }
        }

        if (stateText != null)
        {
            // 텍스트는 cleared 기준으로만 "완료"/"미완료" 표시
            stateText.text = cleared ? "완료" : "미완료";
        }
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

            var targetGraphic = completeButton.targetGraphic as Image;
            if (targetGraphic != null)
            {
                targetGraphic.color = completed ? completedButtonColor : normalButtonColor;
            }
        }

        // ★ 여기
        if (stateText != null)
        {
            stateText.text = completed ? "완료" : "미완료";
        }
    }

    private void OnClickComplete()
    {
        if (owner == null || questData == null)
            return;

        // 이미 보상까지 받은 상태면 무시
        if (isCompleted)
            return;

        // 아직 조건이 안 채워진 상태면 막기
        //  → 이건 SaveData + 외부 이벤트로 SetState(cleared, ...)에서 들어온 값만 믿는다.
        if (!isCleared)
        {
            Debug.Log("[DailyQuestItemUI] 아직 클리어 조건을 만족하지 않은 퀘스트입니다.");
            return;
        }

        // 여기까지 왔으면:
        // - 조건은 이미 충족(cleared == true)
        // - 아직 보상은 안 받음(completed == false)
        owner.OnQuestItemClickedComplete(questData, this);
    }
}

