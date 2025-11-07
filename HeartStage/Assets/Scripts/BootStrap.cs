using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BootStrap : MonoBehaviour
{
    private async UniTask Start()
    {
        await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);
        Debug.Log("씬 로드 시작");
        await Addressables.LoadSceneAsync("testScene");
    }
}
