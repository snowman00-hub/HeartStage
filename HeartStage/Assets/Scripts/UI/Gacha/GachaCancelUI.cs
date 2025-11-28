using UnityEngine;
using UnityEngine.UI;

public class GachaCancelUI : GenericWindow
{
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }
    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }
    private void OnExitButtonClicked()
    {
        Close();
    }
}
