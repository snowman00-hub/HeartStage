using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private TMP_InputField emailInput;
    [SerializeField]
    private TMP_InputField passwdInput;

    [SerializeField]
    private Button loginButton;
    [SerializeField]
    private Button signUpButton;
    [SerializeField]
    private Button anonymouslyLoginButton;

    [SerializeField]
    private TextMeshProUGUI errorText;

    private void OnEnable()
    {
        errorText.text = string.Empty;
    }

    private async UniTaskVoid Start()
    {
        // 로그인 준비 되기 전엔 버튼 막기
        SetButtonsInteractable(false);

        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signUpButton.onClick.AddListener(() => OnSignUpButtonClicked().Forget());
        anonymouslyLoginButton.onClick.AddListener(() => OnAnonyMouslyLoginButtonClicked().Forget());
        SetButtonsInteractable(true);
        UpdateUI();
    }

    // 로그인 시도
    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text;
        string password = passwdInput.text;

        // 로그인 시도중엔 버튼 막기
        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInWithEmailAsync(email, password);
        if (success)
        {

        }
        else
        {
            ShowError("로그인 실패");
        }

        SetButtonsInteractable(true);
        UpdateUI();
    }

    // 회원 가입 시도
    private async UniTaskVoid OnSignUpButtonClicked()
    {
        string email = emailInput.text;
        string password = passwdInput.text;

        // 회원가입 중엔 버튼 막기
        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, password);
        if (success)
        {

        }
        else
        {
            ShowError("회원가입 실패");
        }

        SetButtonsInteractable(true);
        UpdateUI();
    }

    // 익명 로그인 시도
    private async UniTaskVoid OnAnonyMouslyLoginButtonClicked()
    {
        // 익명 로그인 시도 중엔 버튼 막기
        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInAnonymouslyAsync();
        if (success)
        {

        }
        else
        {
            ShowError("익명 로그인 실패");
        }

        SetButtonsInteractable(true);
        UpdateUI();
    }

    // UI 업데이트
    public void UpdateUI()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            return;

        bool isLoggedIn = AuthManager.Instance.IsLoggedIn;
        // 로그인 여부에 따라 로그인UI 뜨고 안뜨게 변경
        loginPanel.SetActive(!isLoggedIn);

        if (isLoggedIn)
        {

        }
        else
        {

        }
    }

    // 버튼 상호작용 세팅
    private void SetButtonsInteractable(bool active)
    {
        loginButton.interactable = active;
        signUpButton.interactable = active;
        anonymouslyLoginButton.interactable = active;
    }

    // 에러 메세지 띄우기
    private void ShowError(string message)
    {
        errorText.text = message;
    }
}