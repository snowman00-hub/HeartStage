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
        Addressables.LoadSceneAsync(StageAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void GoLobby()
    {
        Addressables.LoadSceneAsync(LobbyAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}