using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance;

    public static readonly string StageAddress = "Assets/Scenes/Stage.unity";
    public static readonly string LobbyAddress = "Assets/Scenes/Lobby.unity";
    public static readonly string TestStageAddress = "Assets/Scenes/StageTestScene.ver1.unity";

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
        Time.timeScale = 1.0f;
    }

    // 새로 추가: PlayerPrefs에 StageID 저장 후 Stage 씬 로드
    public void GoStage(int stageId, int startingWave = 1)
    {
        PlayerPrefs.SetInt("SelectedStageID", stageId);
        PlayerPrefs.SetInt("StartingWave", startingWave);
        PlayerPrefs.Save();
        GoStage();
    }

    public void GoTestStage()
    {
        Addressables.LoadSceneAsync(TestStageAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void GoTestStage(int stageId, int startingWave = 1)
    {
        PlayerPrefs.SetInt("SelectedStageID", stageId);
        PlayerPrefs.SetInt("StartingWave", startingWave);
        PlayerPrefs.Save();
        GoTestStage();
    }

    public void GoLobby()
    {
        Addressables.LoadSceneAsync(LobbyAddress, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}