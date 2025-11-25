using System.Text;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultUI : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button reTryButton;


    private GachaResult gachaResult;

    private void Awake()
    {        
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        reTryButton.onClick.AddListener(OnRetryButtonClicked);
    }

    public override void Open()
    {
        base.Open();

        if(GachaUI.gachaResultReciever.HasValue)
        {
            SetGachaResult(GachaUI.gachaResultReciever.Value);
            GachaUI.gachaResultReciever = null; // 결과 사용 후 초기화
        }

        DisPlayResult();
    }
    public override void Close()
    {
        base.Close();
    }

    public void SetGachaResult(GachaResult result)
    {
        gachaResult = result;
    }

    private void DisPlayResult()
    {
        if (gachaResult.characterData == null)
        {
            return;
        }

        var characterData = gachaResult.characterData;

        if(characterNameText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(characterData.char_name);
            characterNameText.text = sb.ToString();
        }

        SetCharacterImage(characterData);
    }

    private void SetCharacterImage(CharacterCSVData characterCsvData)
    {
        if (characterImage == null || string.IsNullOrEmpty(characterCsvData.card_imageName))
        {
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(characterCsvData.card_imageName);
        if (texture != null)
        {
            characterImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError($"캐릭터 이미지 로드 실패: {characterCsvData.card_imageName}");
        }
    }

    // 가지고 있으면 아이템 이미지로 
    private void SetCharacterImageToItem(ItemCSVData itemCsvData)
    {
        if (characterImage == null || string.IsNullOrEmpty(itemCsvData.prefab))
        {
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(itemCsvData.prefab);
        if (texture != null)
        {
            characterImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning($"아이템 이미지를 로드할 수 없습니다: {itemCsvData.prefab}");
        }
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }

    private void OnRetryButtonClicked()
    {
        var gachaResult = GachaManager.Instance.DrawGacha(2); // 2는 캐릭터 가챠 타입 
        
        if(gachaResult.HasValue)
        {
            SetGachaResult(gachaResult.Value);
            DisPlayResult();
        }
        else
        {
            Debug.LogError("가챠 뽑기 실패");
        }

        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }
}
