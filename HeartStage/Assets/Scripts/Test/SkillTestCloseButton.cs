using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SkillTestCloseButton : MonoBehaviour
{
    public Button skillTestCloseButton;
    private void Start()
    {
        skillTestCloseButton.onClick.AddListener(CloseSkillTest);
    }
    public void CloseSkillTest()
    {
        DebugOverlaySceneManager.Instance.CloseSkillTest().Forget();
    }
}