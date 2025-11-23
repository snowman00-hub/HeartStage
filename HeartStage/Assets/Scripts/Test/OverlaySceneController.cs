using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class OverlaySceneController : MonoBehaviour
{
    private Scene baseScene;

    private void Awake()
    {
        baseScene = SceneManager.GetActiveScene();
    }

    public async UniTask OpenOverlay(string overlaySceneName)
    {
        // 1) 오버레이 씬 Additive 로드
        await SceneManager.LoadSceneAsync(overlaySceneName, LoadSceneMode.Additive)
            .ToUniTask();

        // 2) 현재 게임 정지
        Time.timeScale = 0f;

        // 3) 필요하면 오버레이 씬을 Active로(라이트/인풋 기준)
        var overlayScene = SceneManager.GetSceneByName(overlaySceneName);
        if (overlayScene.IsValid())
            SceneManager.SetActiveScene(overlayScene);
    }

    public async UniTask CloseOverlay(string overlaySceneName)
    {
        // 1) 오버레이 씬 언로드
        await SceneManager.UnloadSceneAsync(overlaySceneName)
            .ToUniTask();

        // 2) Active 씬 원복
        if (baseScene.IsValid())
            SceneManager.SetActiveScene(baseScene);

        // 3) 게임 재개
        Time.timeScale = 1f;
    }
}
