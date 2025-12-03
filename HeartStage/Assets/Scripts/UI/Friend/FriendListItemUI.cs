using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button sendEnergyButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button removeFriendButton;

    private string _friendUid;

    public void Setup(string friendUid)
    {
        _friendUid = friendUid;
        nicknameText.text = friendUid; // 기본값

        LoadPublicProfileAsync().Forget();

        sendEnergyButton.onClick.RemoveAllListeners();
        sendEnergyButton.onClick.AddListener(() => OnClickSend().Forget());

        profileButton.onClick.RemoveAllListeners();
        profileButton.onClick.AddListener(() =>
        {
            FriendProfileWindow.Instance?.Open(_friendUid);
        });

        removeFriendButton.onClick.RemoveAllListeners();
        removeFriendButton.onClick.AddListener(() => OnClickRemove().Forget());
    }

    private async UniTaskVoid LoadPublicProfileAsync()
    {
        var data = await PublicProfileService.GetPublicProfileAsync(_friendUid);
        if (data == null) return;

        nicknameText.text = data.nickname;
        var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
        iconImage.sprite = sprite;
    }

    private async UniTaskVoid OnClickSend()
    {
        bool ok = await DreamEnergyGiftService.TrySendDreamEnergyAsync(_friendUid);
        if (ok)
        {
            sendEnergyButton.interactable = false;
        }
        else
        {
            // 실패시 토스트나 팝업 띄우면 됨
        }
    }

    private async UniTaskVoid OnClickRemove()
    {
        bool ok = await FriendService.RemoveFriendAsync(_friendUid);
        if (ok)
        {
            Destroy(gameObject);
        }
    }
}
