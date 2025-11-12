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

        string targetScene = "";
#if UNITY_EDITOR
        string lastScene = EditorPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(lastScene))
            targetScene = lastScene;
#endif

        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = "TestScene";
            Debug.Log("Scene Addressable 찾기 실패");
        }

        Debug.Log($"다음 씬 로드: {targetScene}");
        await Addressables.LoadSceneAsync(targetScene);
    }
}