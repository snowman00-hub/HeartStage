using Cysharp.Threading.Tasks;
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

    public void GoStage(int stageId, int startingWave = 1)
    {
        var gameData = SaveLoadManager.Data;
        gameData.selectedStageID = stageId;
        gameData.startingWave = startingWave;
        SaveLoadManager.SaveToServer().Forget();
        GoStage();
    }

    public void GoTestStage()
    {
        GameSceneManager.ChangeScene(SceneType.TestStageScene);
    }

    public void GoTestStage(int stageId, int startingWave = 1)
    {
        var gameData = SaveLoadManager.Data;
        gameData.selectedStageID = stageId;
        gameData.startingWave = startingWave;
        SaveLoadManager.SaveToServer().Forget();
        GoTestStage();
    }

    public void GoLobby()
    {
        GameSceneManager.ChangeScene(SceneType.LobbyScene);
    }
}