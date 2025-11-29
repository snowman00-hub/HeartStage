using UnityEngine;
using UnityEngine.UI;

public static class CharacterImageHelper
{
    public static void SetCharacterImage(Image targetImage, CharacterData characterData)
    {
        if (targetImage == null) return;

        if (characterData != null && !string.IsNullOrEmpty(characterData.card_imageName))
        {
            var texture2D = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
            if (texture2D != null)
            {
                targetImage.sprite = Sprite.Create(
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );
                return;
            }
        }

        targetImage.sprite = null;
    }
}