using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class LobbySceneController : MonoBehaviour
{
    [Header("로비에 있는 DailyQuests 컴포넌트")]
    public DailyQuests dailyQuestsComponent;
    public WeeklyQuests weeklyQuestsComponent;
    public ArchivementQuests archivementQuestsComponent;

    [Header("공지창 UI (씬에 있는 NoticeWindowRoot)")]
    [SerializeField] private NoticeWindowUI noticeWindow;

    [Header("프로필 UI")]
    [SerializeField] private ProfileWindow profileWindow;


    private async void Awake()
    {
        // 로비에서는 멈출 일 없으면 그냥 1 유지
        Time.timeScale = 1f;

        var tasks = new List<UniTask>();

        // 1) 퀘스트 매니저 로직 초기화 (SaveLoad 끝난 상태라고 가정)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.Initialize();
        }

        // --- 여기서 분기 포인트 ---
        bool needInitDaily = dailyQuestsComponent != null &&
                             !dailyQuestsComponent.IsInitialized;
        bool needInitWeekly = weeklyQuestsComponent != null &&
                             !weeklyQuestsComponent.IsInitialized;
        bool needInitArchivement = archivementQuestsComponent != null &&
                                 !archivementQuestsComponent.IsInitialized;

        if (needInitDaily && needInitWeekly && needInitArchivement)
        {
            // 아직 한 번도 안 초기화된 상태라면 → 진짜 로딩
            var go = dailyQuestsComponent.gameObject;
            bool wasActive = go.activeSelf;

            go.SetActive(true); // 로딩창 뒤에서 몰래 켜기
            await dailyQuestsComponent.InitializeAsync(); // 카드/아이콘 전부 생성
            go.SetActive(wasActive); // 다시 원래 상태(false)로 돌려두기


            go = weeklyQuestsComponent.gameObject;
            wasActive = go.activeSelf;

            go.SetActive(true);
            await weeklyQuestsComponent.InitializeAsync();

            await SyncPublicProfileIfPossible();

            go.SetActive(wasActive); // 다시 원래 상태(false)로 돌려두기
        }
        else
        {
            // 이미 초기화 된 상태라면 여기선 아무것도 안 해도 됨
            Debug.Log("[LobbySceneController] DailyQuests 이미 초기화됨. 로딩 스킵");
        }

        if (noticeWindow != null)
        {
            var go = noticeWindow.gameObject;
            bool wasActive = go.activeSelf;

            go.SetActive(true);                        // 잠깐 켜서 Awake/레이아웃 돌리고
            tasks.Add(noticeWindow.InitializeAsync()); // 리스트 생성
            go.SetActive(wasActive);                   // 다시 원래 상태(false)로
        }

        if (profileWindow != null)
        {
            tasks.Add(profileWindow.PrewarmAsync());
        }

        if (tasks.Count > 0)
            await UniTask.WhenAll(tasks);


        // 여기까지 오면
        // - needInitDaily == true 면 방금 로딩 끝난 상태
        // - needInitDaily == false 면 이미 끝난 상태였음

        // 한 프레임 정도 양보해서 로비 UI Start()들(MoneyUISet 같은거) 한 번 돌리게 함
        await UniTask.Yield();

        // --- 공통: 0.6 → 1.0까지 0.7초 동안 부드럽게 채우기 ---

        const float start = 0.6f;     // 씬 로딩이 0~0.6 쓰고 있다고 가정한 값
        const float end = 1.0f;
        const float duration = 0.7f;  // 0.7초 동안 쭉 채우기

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp01 = Mathf.Clamp01(t / duration);
            float progress = Mathf.Lerp(start, end, lerp01);

            SceneLoader.SetProgressExternal(progress);

            await UniTask.Yield();
        }

        // 3) 안전하게 100% 한 번 더 찍고
        SceneLoader.SetProgressExternal(1.0f);

        // 4) 100% 상태를 잠깐 보여준 다음
        await UniTask.Delay(300, DelayType.UnscaledDeltaTime);

        // 5) 로비 씬 준비 완료 알림 + 로딩창 닫기
        GameSceneManager.NotifySceneReady(SceneType.LobbyScene, 100);
        await SceneLoader.HideLoadingWithDelay(0);
    }

    private async UniTask SyncPublicProfileIfPossible()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);

        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);
        Debug.Log("[Lobby] publicProfiles 동기화 완료");
    }
}
