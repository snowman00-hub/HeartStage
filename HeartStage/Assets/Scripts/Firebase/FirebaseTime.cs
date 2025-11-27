using System;
using Firebase.Database;
using UnityEngine;

public static class FirebaseTime
{
    private static double serverOffsetMs = 0;
    private static bool initialized = false;

    // Firebase 서버 offset 가져오기 (초기화), BootScene에서 하고 있음
    public static void Initialize()
    {
        if (initialized) 
            return;

        initialized = true;

        // Firebase가 제공하는 특수 read-only 전용 경로, 서버 기준 시간과 로컬 시간의 차이(offset) 를 제공
        var offsetRef = FirebaseDatabase.DefaultInstance.GetReference(".info/serverTimeOffset");

        offsetRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogWarning("Failed to read serverTimeOffset. Using local offset 0.");
                return;
            }

            serverOffsetMs = Convert.ToDouble(args.Snapshot.Value);
        };
    }

    // Firebase 서버 시간을 반환 (offset 기반)
    // offset 가져오는게 서버 요청이고, GetServerTime() 자체는 로컬 연산(매우 가벼움)
    public static DateTime GetServerTime()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long serverMs = nowMs + (long)serverOffsetMs; // 로컬 시간 + (서버 시간 - 로컬 시간) = 서버 시간

        return DateTimeOffset.FromUnixTimeMilliseconds(serverMs).LocalDateTime;
    }
}