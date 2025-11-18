using TMPro;
using UnityEngine;

public class StageUI : MonoBehaviour
{
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI remainMonsterCountText;
    
    public void SetWaveCount(int waveCount)
    {
        waveCountText.text = $"Wave {waveCount}";
    }

    public void SetWaveCount(int stageNumber, int waveOrder)
    {
        if (stageNumber == 0)
        {
            waveCountText.text = $"Tutorial {waveOrder}";
        }
        else
        {
            var currentStage = StageManager.Instance.GetCurrentStageData();
            if (currentStage != null)
            {
                waveCountText.text = $"{currentStage.stage_step1}-{currentStage.stage_step2}스테이지\n{waveOrder}웨이브";
            }
        }
    }

    public void SetReaminMonsterCount(int remainMonsterCount)
    {
        remainMonsterCountText.text = $"남은 적군\n{remainMonsterCount}";
    }
}