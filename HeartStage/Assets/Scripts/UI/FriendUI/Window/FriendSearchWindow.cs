using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendSearchWindow : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;             // 전체(배경+패널), 처음엔 비활성

    [Header("연결된 창")]
    [SerializeField] private FriendAddWindow friendAddWindow;
    [SerializeField] private MessageWindow messageWindow;

    [Header("검색 UI")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    private bool _isSearching = false;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(() => OnClickConfirmAsync().Forget());

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // 엔터로도 검색 시도
        if (nicknameInput != null)
            nicknameInput.onSubmit.AddListener(_ => OnClickConfirmAsync().Forget());
    }

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        if (nicknameInput != null)
        {
            nicknameInput.text = string.Empty;
            nicknameInput.ActivateInputField();
        }

        _isSearching = false;

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }

    /// <summary>
    /// 공용 메시지창 호출 헬퍼
    /// </summary>
    private void ShowFail(string message)
    {
        // ✏️ 전체 수정
        if (messageWindow != null)
        {
            messageWindow.OpenFail("검색 실패", message);
        }
        else
        {
            Debug.LogWarning($"[FriendSearchWindow] MessageWindow가 연결되지 않았습니다. 메시지: {message}", this);
        }
    }
    private void ShowSuccess(string message)
    {
        // ✏️ 전체 수정
        if (messageWindow != null)
        {
            messageWindow.OpenSuccess("검색 완료", message);
        }
        else
        {
            Debug.LogWarning($"[FriendSearchWindow] MessageWindow가 연결되지 않았습니다. 메시지: {message}", this);
        }
    }

    /// <summary>
    /// 확인 버튼 클릭 → 닉네임 검색
    /// </summary>
    private async UniTaskVoid OnClickConfirmAsync()
    {
        if (_isSearching) return;
        if (nicknameInput == null)
        {
            Debug.LogWarning("[FriendSearchWindow] nicknameInput이 설정되지 않았습니다.");
            return;
        }

        string nickname = nicknameInput.text?.Trim();
        if (string.IsNullOrWhiteSpace(nickname))
        {
            ShowFail("닉네임을 입력해주세요.");
            return;
        }

        _isSearching = true;
        if (confirmButton != null)
            confirmButton.interactable = false;

        try
        {
            Debug.Log($"[FriendSearchWindow] '{nickname}' 닉네임 검색 시작...");

            // Firebase에서 닉네임 검색
            List<PublicProfileSummary> results = await FriendSearchService.SearchByNicknameAsync(nickname);

            if (results == null || results.Count == 0)
            {
                Debug.Log($"[FriendSearchWindow] '{nickname}' 검색 결과 없음.");
                ShowFail($"'{nickname}' 닉네임을 가진 플레이어를 찾을 수 없습니다.");
            }
            else
            {
                Debug.Log($"[FriendSearchWindow] '{nickname}' 검색 결과: {results.Count}명");

                if (friendAddWindow != null)
                {
                    friendAddWindow.ShowSearchResults(results);
                }
                else
                {
                    Debug.LogError("[FriendSearchWindow] FriendAddWindow가 연결되지 않았습니다!", this);
                }
                // 검색 성공 메시지 (원하면 안 띄워도 됨)
                ShowSuccess("검색된 유저 목록을 확인해주세요.");

                Close();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendSearchWindow] OnClickConfirmAsync Error: {e}");
            ShowFail("검색 중 오류가 발생했습니다.\n잠시 후 다시 시도해주세요.");
        }
        finally
        {
            _isSearching = false;
            if (confirmButton != null)
                confirmButton.interactable = true;
        }
    }
}
