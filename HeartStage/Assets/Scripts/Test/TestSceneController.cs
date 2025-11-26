using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestSceneController : MonoBehaviour
{
    [Header("세팅이 끝나야 하는 애들")]
    public StageSetupWindow stageSetup;
    public TestCharacterDataLaod testcharacterDataLoad;

    private async void Awake()
    {
        Time.timeScale = 0f;

        // 1) 참조 들어올 때까지
        while (stageSetup == null || testcharacterDataLoad == null)
            await UniTask.Yield();

        // 2) 0.9 → 0.99 채우면서 준비 기다리기
        float t = 0f;
        const float fillDuration = 2.0f; // 2초 동안 0.9 → 0.99

        while (!(stageSetup.IsReady && testcharacterDataLoad.IsReady))
        {
            t += Time.unscaledDeltaTime;

            float lerp01 = Mathf.Clamp01(t / fillDuration);
            // ★ 최대를 0.99로 제한
            float progress = Mathf.Lerp(0.9f, 0.99f, lerp01);

            SceneLoader.SetProgressExternal(progress);

            await UniTask.Yield();
        }

        // 3) 진짜로 둘 다 준비 끝난 시점에서만 100% 찍기
        SceneLoader.SetProgressExternal(1.0f);

        // 4) 100% 상태를 살짝 보여주고 로딩창 끄기
        GameSceneManager.NotifySceneReady(SceneType.StageScene, 100);
        await UniTask.Yield();
    }
}
