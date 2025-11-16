using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootStrap : MonoBehaviour
{
    private const string Key = "LastPlayedScenePath";

    // Scene Addressable 주소 바꾸지 말기
    private async UniTask Start()
    {
        await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);

        await ResourceManager.Instance.PreloadLabelAsync("SFX"); // 사운드 추가 로드
        await ResourceManager.Instance.PreloadLabelAsync("BGM");

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