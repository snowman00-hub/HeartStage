using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class DebugOverlaySceneManager : MonoBehaviour
{
    public static DebugOverlaySceneManager Instance;
    private float prevTimeScale = 1f;

    [Header("Skill Test Scene Addressables Key")]
    [SerializeField] private string skillTestAddress = "Assets/Scenes/SkillTestScene.unity";

    [Header("Exclude roots from freezing (Stage scene only)")]
    [SerializeField] private List<GameObject> excludeRoots = new();

    private Scene frozenScene;
    private readonly List<GameObject> frozenRoots = new();
    private AsyncOperationHandle<SceneInstance>? overlayHandle;
    private bool isFrozen;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // 반드시 전역 유지
    }

    private void FreezeActiveScene()
    {
        if (isFrozen) return;

        frozenScene = SceneManager.GetActiveScene();
        frozenRoots.Clear();
        frozenScene.GetRootGameObjects(frozenRoots);

        foreach (var go in frozenRoots)
        {
            if (go == null) continue;
            if (excludeRoots.Contains(go)) continue;
            go.SetActive(false); // StageTestScene.ver1 “진짜 정지”
        }

        isFrozen = true;
    }

    private void Unfreeze()
    {
        if (!isFrozen) return;

        foreach (var go in frozenRoots)
        {
            if (go == null) continue;
            if (excludeRoots.Contains(go)) continue;
            go.SetActive(true);
        }

        frozenRoots.Clear();
        isFrozen = false;
    }

    public async UniTask OpenSkillTest()
    {
        FreezeActiveScene();

        //  timeScale 임시 복구
        prevTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        var handle = Addressables.LoadSceneAsync(skillTestAddress, LoadSceneMode.Additive);
        overlayHandle = handle;

        var sceneInstance = await handle.ToUniTask();
        SceneManager.SetActiveScene(sceneInstance.Scene);
    }
    public async UniTask CloseSkillTest()
    {
        if (overlayHandle == null) return;

        await Addressables.UnloadSceneAsync(overlayHandle.Value, true).ToUniTask();
        overlayHandle = null;

        if (frozenScene.IsValid())
            SceneManager.SetActiveScene(frozenScene);

        Unfreeze();

        //  원래 timeScale로 복구
        Time.timeScale = prevTimeScale;
    }
}

