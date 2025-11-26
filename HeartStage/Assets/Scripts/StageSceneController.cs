using Cysharp.Threading.Tasks;
using UnityEngine;
public class StageSceneController : MonoBehaviour
{
    [Header("세팅이 끝나야 하는 애들")]
    public StageSetupWindow stageSetup;
    public OwnedCharacterSetup ownedSetup;
    // 필요하면 다른 애도 추가: public SomeOtherSetup otherSetup;

    private async void Awake()
    {
        Time.timeScale = 0f;
        // 참조 들어올 때까지
        while (stageSetup == null || ownedSetup == null)
            await UniTask.Yield();

        // 둘 다 준비될 때까지 기다림
        await UniTask.WaitUntil(() =>
            stageSetup.IsReady &&
            ownedSetup.IsReady
        );

        // 1) 먼저 로딩창 끄기
        GameSceneManager.NotifySceneReady(SceneType.StageScene, 0);

        // 2) 로딩이 완전히 내려가도록 한 프레임 넘기고
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
    }
}