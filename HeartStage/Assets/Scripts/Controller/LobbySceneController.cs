using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbySceneController : MonoBehaviour
{
    [Header("필요하면 인스펙터에 LobbyManager 끼워넣기")]
    public LobbyManager lobbyManager;

    private async void Awake()
    {
        // 로비에서는 게임 멈출 필요 없으면 그냥 1 유지
        Time.timeScale = 1f;

        // 1) LobbyManager 참조 들어올 때까지 대기
        while (lobbyManager == null)
            await UniTask.Yield();

        // 혹시 Start()에서 MoneyUISet까지 끝나길 한 프레임 정도 기다리고 싶으면
        await UniTask.Yield();

        // 2) 0.6 → 1.0까지 부드럽게 채우기 (로비 전용 연출 구간)
        const float start = 0.6f;   // 씬 로딩이 0~0.6 쓰고 있으니까
        const float end = 1.0f;
        const float duration = 0.7f; // 0.7초 동안 쭉 채우기 (원하면 조절)

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
}
