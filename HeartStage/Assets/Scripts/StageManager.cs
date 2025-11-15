using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageUI StageUI;
    public Slider expSlider;

    // 스테이지 관련 추가 한 것
    private int stageNumber = 1;
    private int waveOrder = 1;

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

    // 스테이지 관련 추가 한 것
    public void SetWaveInfo(int stage, int wave)
    {
        stageNumber = stage;
        waveOrder = wave;
        waveCount = wave; // 기존 호환성을 위해 유지

        // StageUI의 SetWaveCount 메서드가 두 개의 매개변수를 받는지 확인 필요
        if (StageUI != null)
        {
            StageUI.SetWaveCount(stageNumber, waveOrder);
        }
    }

    // 경험치 얻기
    public void ExpGet(int value)
    {
        expSlider.value += value;
        if(expSlider.maxValue == expSlider.value)
        {
            expSlider.value = 0f;
            LevelUp();
        }
    }

    // 레벨업
    public void LevelUp()
    {
        Debug.Log("Level Up!");
    }
}