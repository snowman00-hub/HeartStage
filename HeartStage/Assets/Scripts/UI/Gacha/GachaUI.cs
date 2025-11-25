using UnityEngine;
using UnityEngine.UI;

public class GachaUI : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private int currentGachaTypeId = 2; // 기본값 2: 캐릭터 가챠

    [Header("Button")]
    [SerializeField] private Button percentageInfoButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button gachaFiveButton;

    // 간단한 정적 변수로 결과 전달
    public static GachaResult? gachaResultReciever; 

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void Awake()
    {
        percentageInfoButton.onClick.AddListener(OnGachaPercentageInfoButtonClicked);
        gachaButton.onClick.AddListener(OnGachaButtonOnClicked);
    }

    private void OnGachaPercentageInfoButtonClicked()
    {
        WindowManager.Instance.OpenOverlay(WindowType.GachaPercentage);
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }

    public void OnGachaButtonOnClicked()
    {
        var gachaResult = GachaManager.Instance.DrawGacha(currentGachaTypeId);

        if (gachaResult.HasValue)
        {
            // 정적 변수에 결과 저장
            gachaResultReciever = gachaResult.Value;

            // 결과창 열기
            WindowManager.Instance.OpenOverlay(WindowType.GachaResult);
        }
        else
        {
            Debug.LogError("가챠 뽑기 실패");
        }

        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }
}