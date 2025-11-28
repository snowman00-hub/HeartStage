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

    private void OnClickGoTestScene()
    {
        LoadSceneManager.Instance.GoTestStage(601);
    }
}
