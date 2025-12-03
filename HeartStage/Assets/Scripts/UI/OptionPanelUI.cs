using UnityEngine;
using UnityEngine.UI;

public class OptionPanelUI : MonoBehaviour
{
    [SerializeField] private Button mailButton;
    [SerializeField] private Button settingButton;

    private void Awake()
    {
        mailButton.onClick.AddListener(OnMailButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
    }

    private void OnMailButtonClicked()
    {        
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
        WindowManager.Instance.OpenOverlay(WindowType.MailUI);
    }

    private void OnSettingButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
        WindowManager.Instance.OpenOverlay(WindowType.SettingPanel);
    }
}
