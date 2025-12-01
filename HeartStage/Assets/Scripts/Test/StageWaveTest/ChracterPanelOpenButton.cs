using UnityEngine;
using UnityEngine.UI;

public class ChracterPanelOpenButton : MonoBehaviour
{
    public CharacterTestPanel characterPanel;
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(OnClick_OpenCharacter);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick_OpenCharacter);
    }
    void OnClick_OpenCharacter()
    {
        characterPanel.Open();
    }
}
