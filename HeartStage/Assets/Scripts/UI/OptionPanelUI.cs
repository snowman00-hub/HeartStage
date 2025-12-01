using UnityEngine;
using UnityEngine.UI;

public class OptionPanelUI : MonoBehaviour
{
    [SerializeField] private Button mailButton;

    private void Awake()
    {
        mailButton.onClick.AddListener(OnMailButtonClicked);
    }

    private void OnMailButtonClicked()
    {        
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
        WindowManager.Instance.OpenOverlay(WindowType.MailUI);
    }
}
