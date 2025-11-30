using UnityEngine;
using UnityEngine.UI;

public class GoStageWaveTest : MonoBehaviour
{
    [SerializeField] private Button button;
    private void Start()
    {
        button.onClick.AddListener(goStageWaveTestClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(goStageWaveTestClick);
    }


    private void goStageWaveTestClick()
    {
        GameSceneManager.ChangeScene(SceneType.TestStageWaveScene);
    }
}
