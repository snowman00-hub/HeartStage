using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageSceneController : MonoBehaviour
{
    [Header("세팅이 끝나야 하는 애들")]
    public StageSetupWindow stageSetup;
    public OwnedCharacterSetup ownedSetup;

    private async void Awake()
    {
        // 전투 시작 전에는 일단 멈춰둠
        Time.timeScale = 0f;

        // 1) 참조 들어올 때까지 (Awake/Start 순서 안전망)
        while (stageSetup == null || ownedSetup == null)
            await UniTask.Yield();

        // 2) 둘 다 IsReady 될 때까지 대기
        while (!(stageSetup.IsReady && ownedSetup.IsReady && PoolManager.Instance.IsSpawned))
            await UniTask.Yield();

        // 3) 진짜로 둘 다 준비 끝난 시점에서만 100% 찍기
        SceneLoader.SetProgressExternal(1.0f);

        // 4) 100% 상태를 잠깐 보여주고
        await UniTask.Delay(300, DelayType.UnscaledDeltaTime);

        // 5) 게임 씬 준비 완료 알림 (기존 로직 유지)
        GameSceneManager.NotifySceneReady(SceneType.StageScene, 100);

        // 6) 로딩 UI 닫기 (혹시 GameSceneManager에서 닫으면 여기 제거해도 됨)
        await SceneLoader.HideLoadingWithDelay(0);
    }
}
