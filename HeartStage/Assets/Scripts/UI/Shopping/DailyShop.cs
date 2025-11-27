using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyShop : MonoBehaviour
{
    public TextMeshProUGUI remainTimeText;
    public List<ShopItemSlot> dailyItemSlots;

    [Header("Reset Interval (seconds)")]
    public int resetIntervalSeconds = 86400;
    // 테스트: 10초 → 10
    // 실제: 24시간 → 86400

    // 다음에 상점이 리셋될 남은 시간
    private DateTime nextResetTime;
    private const string ResetKey = "DailyShopNextResetTime";

    private void Start()
    {
        LoadOrInitResetTime();
        UpdateUI();
    }

    private void Update()
    {
        UpdateCountdown();
    }

    private void LoadOrInitResetTime()
    {
        DateTime serverNow = FirebaseTime.GetServerTime();

        // 기존 저장값 있는지 확인
        if (PlayerPrefs.HasKey(ResetKey))
        {
            long binary = Convert.ToInt64(PlayerPrefs.GetString(ResetKey));
            nextResetTime = DateTime.FromBinary(binary);
        }
        else
        {
            nextResetTime = serverNow.AddSeconds(resetIntervalSeconds); 
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
        }

        // 서버 기준으로 리셋 시점 도달했는지 체크
        if (serverNow >= nextResetTime) 
        {
            ResetDailyShop();
            nextResetTime = serverNow.AddSeconds(resetIntervalSeconds); 
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
        }
    }

    // 서버 기준 남은 시간 계산
    private void UpdateCountdown()
    {
        DateTime serverNow = FirebaseTime.GetServerTime();
        TimeSpan remain = nextResetTime - serverNow; 

        if (remain.TotalSeconds <= 0)
        {
            ResetDailyShop();
            nextResetTime = serverNow.AddSeconds(resetIntervalSeconds);
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
            remain = nextResetTime - serverNow;
        }

        remainTimeText.text = $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
    }

    // 판매품 리셋 (현재 조각만 판매 중)
    public void ResetDailyShop()
    {
        var randIds = DataTableManager.ShopTable.GetRandomThreePieceIds();
        for (int i = 0; i < 3; i++)
        {
            dailyItemSlots[i].Init(randIds[i], false);
        }
        // 구매 확인 창이 열려 있는 도중 바뀌는 경우 대처
        PurchaseConfirmPanel.Instance.Close();
    }
    
    // 남은 시간 Text 업데이트
    private void UpdateUI()
    {
        DateTime serverNow = FirebaseTime.GetServerTime(); 
        TimeSpan remain = nextResetTime - serverNow; 

        remainTimeText.text = $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
    }
}