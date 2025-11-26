using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageSceneController : MonoBehaviour
{
    [Header("세팅이 끝나야 하는 애들")]
    public StageSetupWindow stageSetup;
    public OwnedCharacterSetup ownedSetup;

    private async void Awake()
    {
        Time.timeScale = 0f;

        while (stageSetup == null || ownedSetup == null)
            await UniTask.Yield();

        float t = 0f;
        const float fillDuration = 2.0f;

        while (!(stageSetup.IsReady && ownedSetup.IsReady))
        {
            t += Time.unscaledDeltaTime;

            float lerp01 = Mathf.Clamp01(t / fillDuration);
            float progress = Mathf.Lerp(0.9f, 1.0f, lerp01);

            SceneLoader.SetProgressExternal(progress);

            await UniTask.Yield();
        }

        SceneLoader.SetProgressExternal(1.0f);

        GameSceneManager.NotifySceneReady(SceneType.StageScene, 100);
        await UniTask.Yield();
    }

}
