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

    public void SetReaminMonsterCount(int remainMonsterCount)
    {
        remainMonsterCountText.text = $"{remainMonsterCount}";
    }
}