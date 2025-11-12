using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootStrap : MonoBehaviour
{
    private const string Key = "LastPlayedScenePath";

    private async UniTask Start()
    {
        await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);
        await DataTableManager.Initialization;

        string targetScene = "Assets/Scenes/feature-tower.unity";

#if UNITY_EDITOR
        string lastScene = EditorPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(lastScene) && lastScene != "Assets/Scenes/bootScene.unity")
            targetScene = lastScene;
#endif
        
        Debug.Log($"다음 씬 로드: {targetScene}");
        await Addressables.LoadSceneAsync(targetScene);
    }
}