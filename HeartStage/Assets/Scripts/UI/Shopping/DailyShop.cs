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

    // firebase 추가시 서버 시간으로 바꾸기
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

    void LoadOrInitResetTime()
    {
        if (PlayerPrefs.HasKey(ResetKey))
        {
            long binary = Convert.ToInt64(PlayerPrefs.GetString(ResetKey));
            nextResetTime = DateTime.FromBinary(binary);
        }
        else
        {
            nextResetTime = DateTime.Now.AddSeconds(resetIntervalSeconds);
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
        }

        if (DateTime.Now >= nextResetTime)
        {
            ResetDailyShop();
            nextResetTime = DateTime.Now.AddSeconds(resetIntervalSeconds);
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
        }
    }

    void UpdateCountdown()
    {
        TimeSpan remain = nextResetTime - DateTime.Now;

        if (remain.TotalSeconds <= 0)
        {
            ResetDailyShop();
            nextResetTime = DateTime.Now.AddSeconds(resetIntervalSeconds);
            PlayerPrefs.SetString(ResetKey, nextResetTime.ToBinary().ToString());
            remain = nextResetTime - DateTime.Now;
        }

        remainTimeText.text = $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
    }

    public void ResetDailyShop()
    {
        var randIds = DataTableManager.ShopTable.GetRandomThreePieceIds();
        for (int i = 0; i < 3; i++)
        {
            dailyItemSlots[i].Init(randIds[i], false);
        }
    }

    private void UpdateUI()
    {
        TimeSpan remain = nextResetTime - DateTime.Now;
        remainTimeText.text = $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
    }
}
