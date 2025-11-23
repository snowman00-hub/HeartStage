using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SkillTestOpenButton : MonoBehaviour
{
    public Button skillTestOpenButton;
    private void Start()
    {
        skillTestOpenButton.onClick.AddListener(OpenSkillTest);
    }
    public void OpenSkillTest()
    {
        DebugOverlaySceneManager.Instance.OpenSkillTest().Forget();
    }
}