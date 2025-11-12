using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageUI StageUI;

    private int waveCount = 1;
    public int WaveCount
    {
        get { return waveCount; }
        set
        {
            waveCount = value;
            StageUI.SetWaveCount(waveCount);
        }
    }

    private int remainMonsterCount;
    public int RemainMonsterCount
    {
        get { return remainMonsterCount; }
        set
        {
            remainMonsterCount = value;
            StageUI.SetReaminMonsterCount(remainMonsterCount);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void GoLobby()
    {
        LoadSceneManager.Instance.GoLobby();
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    private void OnDestroy()
    {
        SetTimeScale(1f);
    }
}