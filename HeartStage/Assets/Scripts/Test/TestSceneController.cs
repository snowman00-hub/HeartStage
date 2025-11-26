using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestSceneController : MonoBehaviour
{
    [Header("세팅이 끝나야 하는 애들")]
    public StageSetupWindow stageSetup;
    public TestCharacterDataLaod testcharacterDataLoad;

    private async void Awake()
    {
        // 테스트 씬에서도 전투 시작 전엔 멈추고 시작
        Time.timeScale = 0f;

        // 1) 참조 들어올 때까지
        while (stageSetup == null || testcharacterDataLoad == null)
            await UniTask.Yield();

        // 2) 둘 다 IsReady 될 때까지 대기
        while (!(stageSetup.IsReady && testcharacterDataLoad.IsReady))
            await UniTask.Yield();

        // 3) 진짜 준비 끝난 시점에서만 100% 찍기
        SceneLoader.SetProgressExternal(1.0f);

        // 4) 100% 상태를 잠깐 보여주고
        await UniTask.Delay(300, DelayType.UnscaledDeltaTime);

        // 5) 게임 씬 준비 완료 알림 (Test용 SceneType은 네 enum 그대로)
        GameSceneManager.NotifySceneReady(SceneType.TestStageScene, 100);

        // 6) 로딩 UI 닫기 (Stage씬이랑 동일하게)
        await SceneLoader.HideLoadingWithDelay(0);
    }
}
