using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterBuffController : MonoBehaviour
{
    private CharacterData data;

    public void SetData(CharacterData data)
    {
        this.data = data;
    }

    // 적용 예시 실제 아님.
    public async void ApplyBuffAsync(string stat, float multiplier, float duration)
    {
        // 적용
        switch (stat)
        {
            case "atk_dmg":
                data.atk_dmg *= multiplier;
                break;
            case "bullet_speed":
                data.bullet_speed *= multiplier;
                break;
            case "atk_range":
                data.atk_range *= multiplier;
                break;
        }

        // duration 동안 대기
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));

        // 원상복귀
        switch (stat)
        {
            case "atk_dmg":
                data.atk_dmg /= multiplier;
                break;
            case "bullet_speed":
                data.bullet_speed /= multiplier;
                break;
            case "atk_range":
                data.atk_range /= multiplier;
                break;
        }
    }
}