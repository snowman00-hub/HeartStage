using UnityEngine;
using UnityEngine.UI;

public class TestWindow : GenericWindow
{
    [SerializeField] WindowManager windowManager;
    [SerializeField] private Button testButton;
    private void Awake()
    {
        testButton.onClick.AddListener(OnTestButtonClicked);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void OnTestButtonClicked()
    {
        windowManager.Open(WindowType.Test2Window);
    }
}
