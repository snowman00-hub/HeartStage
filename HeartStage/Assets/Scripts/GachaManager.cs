using System.Collections.Generic;
using UnityEngine;
public struct GachaResult
{
    public GachaData gachaData;
    public CharacterCSVData characterData;

    public GachaResult(GachaData gachaData, CharacterCSVData characterData)
    {
        this.gachaData = gachaData;
        this.characterData = characterData;
    }
}

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GachaResult? DrawGacha(int gachaTypeId)
    {
        // 가챠 타입 정보 가져오기
        var gachaType = DataTableManager.GachaTypeTable.Get(gachaTypeId);
        if (gachaType == null)
        {
            Debug.LogError($"가챠 타입 {gachaTypeId}를 찾을 수 없습니다.");
            return null;
        }

        // 재화 차감 추가해야함
       

        // 해당 타입의 가챠 아이템들 가져오기
        var gachaItems = GachaTable.GetGachaByType(gachaTypeId);
        if (gachaItems == null || gachaItems.Count == 0)
        {
            Debug.LogError($"가챠 타입 {gachaTypeId}의 아이템이 없습니다.");
            return null;
        }

        // 확률에 따른 랜덤 뽑기
        var selectedItem = DrawRandomItem(gachaItems);
        if (selectedItem == null)
        {
            Debug.LogError("가챠 뽑기에 실패했습니다.");
            return null;
        }

        // 결과 반환
        return new GachaResult
        {
            gachaData = selectedItem,
            characterData = DataTableManager.CharacterTable.Get(selectedItem.Gacha_item)
        };
    }

    private GachaData DrawRandomItem(List<GachaData> gachaItems)
    {
        // 전체 확률의 합 계산
        int totalPercentage = 0;
        foreach (var item in gachaItems)
        {
            totalPercentage += item.Gacha_per;
        }

        // 랜덤 값 생성
        int randomValue = Random.Range(0, totalPercentage);

        // 확률에 따른 아이템 선택
        int currentSum = 0;
        foreach (var item in gachaItems)
        {
            currentSum += item.Gacha_per;
            if (randomValue < currentSum)
            {
                return item;
            }
        }

        // 예외 상황 (마지막 아이템 반환)
        return gachaItems[gachaItems.Count - 1];
    }
}

