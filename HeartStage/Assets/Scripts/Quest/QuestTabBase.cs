using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Daily / Weekly / Achievement 공통 UI 탭 베이스.
/// - 스크롤뷰 Content 밑에 프리팹 깔아서 리스트 만드는 공통 로직
/// - 각 탭은 GetQuestDefinitions / SetupItemUI / RefreshAllItemStatesFromSave 만 구현하면 됨.
/// </summary>
public abstract class QuestTabBase<TItemUI> : MonoBehaviour where TItemUI : MonoBehaviour
{
    [Header("퀘스트 리스트 (ScrollView Content)")]
    [SerializeField] protected Transform questListContent;   // ScrollView/Viewport/Content
    [SerializeField] protected TItemUI questItemPrefab;      // 아이템 프리팹

    // 실제로 생성된 UI 아이템들
    protected readonly List<TItemUI> questItems = new List<TItemUI>();

    // 한 번 초기화했는지 여부(각 탭에서 관리)
    public bool IsInitialized { get; protected set; }

    // 이 탭에서 보여줄 퀘스트 정의 리스트(QuestTable/QuestManager 등에서 가져오기).
    protected abstract IReadOnlyList<QuestData> GetQuestDefinitions();

    /// 개별 아이템 UI 초기화.
    /// - Daily/Weekly/Achievement 탭에서 상태(cleared/completed 등) 계산해서 여기에서 Init 호출.
    protected abstract void SetupItemUI(TItemUI ui, QuestData data);

    /// SaveData 기준으로 각 아이템 상태를 다시 반영해주는 함수.
    /// 탭이 켜질 때 / 저장 불러온 뒤 등에 호출.
    public abstract void RefreshAllItemStatesFromSave();

    /// [전체 보상 받기]용 (Daily는 구현, Weekly/Achievement는 필요 없으면 비워도 됨)
    public virtual void ClaimAllAvailableRewards() { }

    /// 공통: 퀘스트 정의 리스트를 바탕으로 스크롤뷰 안에 아이템들 다시 생성.
    protected void RebuildQuestItems()
    {
        if (questListContent == null || questItemPrefab == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Quest 리스트 생성에 필요한 참조(questListContent, questItemPrefab)가 없습니다.");
            return;
        }

        // 기존 것들 정리
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        questItems.Clear();

        var questDefs = GetQuestDefinitions();
        if (questDefs == null || questDefs.Count == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] 표시할 퀘스트가 없습니다.");
            return;
        }

        foreach (var quest in questDefs)
        {
            if (quest == null)
                continue;

            var item = Instantiate(questItemPrefab, questListContent);
            SetupItemUI(item, quest);   // 각 탭에서 상태 계산 + Init 호출
            questItems.Add(item);
        }
    }

    /// 기본 OnEnable: 탭이 켜질 때 SaveData 기준으로 상태만 다시 반영.
    /// Daily는 이벤트 등록도 같이 하고 싶으면 override 해서 base.OnEnable() 호출.
    protected virtual void OnEnable()
    {
        RefreshAllItemStatesFromSave();
    }
}
