using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks; // UniTask 쓸 경우

public class ResetButton : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private StageSetupWindow stageSetupWindow;

    private void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);
        else
            Debug.LogError("[ResetButton] resetButton not assigned.");

        if (monsterSpawner == null)
            Debug.LogError("[ResetButton] monsterSpawner not assigned.");

        if (stageSetupWindow == null)
            Debug.LogError("[ResetButton] stageSetupWindow not assigned.");
    }

    private async void OnResetButtonClicked()
    {
        if (monsterSpawner == null || StageManager.Instance == null)
            return;

        try
        {
            Time.timeScale = 0f;
            stageSetupWindow.gameObject.SetActive(true);
            await monsterSpawner.ChangeStage(601);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            resetButton.interactable = true;
        }
    }
}

