using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance;

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
        GameSceneManager.ChangeScene(SceneType.StageScene);
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
        GameSceneManager.ChangeScene(SceneType.TestStageScene);
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
        GameSceneManager.ChangeScene(SceneType.LobbyScene);
    }
}