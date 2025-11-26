using UnityEngine;

public class TestSceneController : MonoBehaviour
{
    private void Start()
    {
        GameSceneManager.NotifySceneReady(SceneType.TestStageScene, 0);
    }
}
