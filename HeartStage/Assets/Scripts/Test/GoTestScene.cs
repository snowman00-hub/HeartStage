using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GoTestScene : MonoBehaviour
{
    [SerializeField] private Button goTestSceneButton;

    // Addressables Groups에 등록된 씬을 여기 슬롯에 드래그
    [SerializeField] private AssetReference testScene;

    private void OnEnable()
    {
        if (goTestSceneButton != null)
            goTestSceneButton.onClick.AddListener(OnClickGoTestScene);
    }

    private void OnDisable()
    {
        if (goTestSceneButton != null)
            goTestSceneButton.onClick.RemoveListener(OnClickGoTestScene);
    }

    private async void OnClickGoTestScene()
    {
        if (testScene == null || !testScene.RuntimeKeyIsValid())
        {
            Debug.LogError("[GoTestScene] testScene Addressable reference is not valid.");
            return;
        }

        // Addressables 씬 로드 (Single이면 기존 씬 자동 언로드)
        var handle = Addressables.LoadSceneAsync(
            testScene,
            LoadSceneMode.Single,
            activateOnLoad: true
        ); // :contentReference[oaicite:1]{index=1}

        await handle.ToUniTask();

        if (handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError($"[GoTestScene] Load failed: {handle.OperationException}");
    }
}
