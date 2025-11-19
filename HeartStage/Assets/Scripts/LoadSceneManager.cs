using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance;

    public static readonly string StageAddress = "Assets/Scenes/Stage.unity";
    public static readonly string LobbyAddress = "Assets/Scenes/Lobby.unity";

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void GoStage()
    {
        // 씬 전환 직전에 PoolManager 정리
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.CleanupForSceneTransition();
        }

        Addressables.LoadSceneAsync(StageAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
        Time.timeScale = 1.0f;
    }

    public void GoLobby()
    {
        // 씬 전환 직전에 PoolManager 정리
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.CleanupForSceneTransition();
        }

        Addressables.LoadSceneAsync(LobbyAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}