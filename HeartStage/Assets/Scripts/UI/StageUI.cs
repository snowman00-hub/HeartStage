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
            waveCountText.text = $"{stageNumber} - {waveOrder}";
        }
    }

    public void SetReaminMonsterCount(int remainMonsterCount)
    {
        remainMonsterCountText.text = $"{remainMonsterCount}";
    }


}