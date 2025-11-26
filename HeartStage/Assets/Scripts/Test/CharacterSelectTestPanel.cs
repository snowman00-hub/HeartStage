using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterSelectTestPanel : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public Image attributeIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI idolPowerCount;
    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public Image cardImage;

    /// <summary>
    /// 편의용: 기다릴 필요 없을 때
    /// </summary>
    public void Init(CharacterData characterData)
    {
        InitAsync(characterData).Forget();
    }

    /// <summary>
    /// 텍스트 + 카드 이미지까지 모두 세팅되는 시점까지 기다리는 async Init
    /// </summary>
    public async UniTask InitAsync(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogWarning("[CharacterSelectPanel] characterData is null", this);
            return;
        }

        // --- 텍스트 세팅 (null 방어) ---
        if (rankText != null)
            rankText.text = $"{characterData.char_rank}";

        if (characterName != null)
            characterName.text = characterData.char_name;

        if (idolPowerCount != null)
            idolPowerCount.text = $"{characterData.GetTotalPower()}";

        if (levelText != null)
            levelText.text = $"LV {characterData.char_lv}";

        // expSlider는 나중에 경험치 시스템 붙일 때 세팅

        // --- 카드 이미지 로드 ---
        if (cardImage == null)
            return;

        Texture2D texture2D = null;

        // 1) ResourceManager가 있으면 먼저 시도 (실제 게임용)
        if (!string.IsNullOrEmpty(characterData.card_imageName) &&
            ResourceManager.Instance != null)
        {
            texture2D = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
        }

        // 2) ResourceManager에서 못 가져왔거나, TestScene처럼 ResourceManager가 없는 경우
        //    → Addressables에서 직접 로드 (이 시간이 바로 "이미지 로딩 시간")
        if (texture2D == null && !string.IsNullOrEmpty(characterData.card_imageName))
        {
            AsyncOperationHandle<Texture2D> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<Texture2D>(characterData.card_imageName);
                texture2D = await handle.Task;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(
                    $"[CharacterSelectPanel] Failed to load card texture: {characterData.card_imageName}\n{e}",
                    this
                );
            }
            finally
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
        }

        if (texture2D != null)
        {
            cardImage.sprite = Sprite.Create(
                texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        else
        {
            Debug.LogWarning(
                $"[CharacterSelectPanel] Texture not found: {characterData.card_imageName}",
                this
            );
        }
    }
}
