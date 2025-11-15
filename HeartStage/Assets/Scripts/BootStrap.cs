using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootStrap : MonoBehaviour
{
    private const string Key = "LastPlayedScenePath";

    // Scene 새로 복사했으면 Addressable 체크하고 플레이 하기, 등록 안되면 오류 뜸!
    // Scene Addressable 주소 바꾸지 말기, 그냥 체크만 하기
    private async UniTask Start()
    {
        // 비동기로 미리 해야하는 작업들 있으면 가능한 부트 씬에서 하고 해당 씬에선 동기로 쓰기
        await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);
        await DataTableManager.Initialization;

        string targetScene = "Assets/Scenes/Lobby.unity";

#if UNITY_EDITOR
        string lastScene = EditorPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(lastScene) && lastScene != "Assets/Scenes/bootScene.unity")
            targetScene = lastScene;
#endif
        
        Debug.Log($"다음 씬 로드: {targetScene}");
        await Addressables.LoadSceneAsync(targetScene);
    }
}