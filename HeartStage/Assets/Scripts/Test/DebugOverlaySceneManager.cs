using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class DebugOverlaySceneManager : MonoBehaviour
{
    public static DebugOverlaySceneManager Instance;
    private float prevTimeScale = 1f;

    [SerializeField] private string skillTestAddress = "Assets/Scenes/SkillTestScene.unity";
    [SerializeField] private List<GameObject> excludeRoots = new();

    private Scene frozenScene;
    private readonly List<GameObject> frozenRoots = new();

    // 추가: 루트별 원래 active 상태 저장
    private readonly Dictionary<GameObject, bool> frozenRootStates = new();

    private AsyncOperationHandle<SceneInstance>? overlayHandle;
    private bool isFrozen;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void FreezeActiveScene()
    {
        if (isFrozen) return;

        frozenScene = SceneManager.GetActiveScene();
        frozenRoots.Clear();
        frozenRootStates.Clear();

        frozenScene.GetRootGameObjects(frozenRoots);

        foreach (var go in frozenRoots)
        {
            if (go == null) continue;
            if (excludeRoots.Contains(go)) continue;

            // 원래 상태 저장
            frozenRootStates[go] = go.activeSelf;

            // 꺼버림
            go.SetActive(false);
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

            // ✅ “무조건 true”가 아니라 원래 상태로 복구
            if (frozenRootStates.TryGetValue(go, out bool wasActive))
                go.SetActive(wasActive);
            else
                go.SetActive(true); // 혹시 모를 안전망
        }

        frozenRoots.Clear();
        frozenRootStates.Clear();
        isFrozen = false;
    }

    public async UniTask OpenSkillTest()
    {
        FreezeActiveScene();

        prevTimeScale = Time.timeScale; // 기록만
        // Time.timeScale = 1f;  <-- (구버전에 있으면 제거)

        var handle = Addressables.LoadSceneAsync(skillTestAddress, LoadSceneMode.Additive);
        overlayHandle = handle;

        var sceneInstance = await handle.ToUniTask();
        SceneManager.SetActiveScene(sceneInstance.Scene);

        ApplyUnscaledToScene(sceneInstance.Scene);
    }

    public async UniTask CloseSkillTest()
    {
        if (overlayHandle == null) return;

        await Addressables.UnloadSceneAsync(overlayHandle.Value, true).ToUniTask();
        overlayHandle = null;

        if (frozenScene.IsValid())
            SceneManager.SetActiveScene(frozenScene);

        Unfreeze();
        Time.timeScale = prevTimeScale;
    }

    private void ApplyUnscaledToScene(Scene scene) { /* 너가 쓰던 그거 */ }
}
