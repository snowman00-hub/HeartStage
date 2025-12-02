using System;

public static class MaintenanceUtil
{
    public static bool IsMaintenanceNow(MaintenanceData m, DateTimeOffset now)
    {
        if (m == null)
            return false;

        bool inSchedule = false;

        // 1) 시간 기반 스케줄 먼저 체크 (startAt ~ endAt)
        if (!string.IsNullOrEmpty(m.startAt) &&
            DateTimeOffset.TryParse(m.startAt, out var start))
        {
            if (string.IsNullOrEmpty(m.endAt))
            {
                // endAt 없으면 start 이후 계속 점검
                inSchedule = now >= start;
            }
            else if (DateTimeOffset.TryParse(m.endAt, out var end))
            {
                // start <= now <= end 이면 점검
                inSchedule = now >= start && now <= end;
            }
        }

        // ⬅ 스케줄 안에 들어가면, active가 false여도 무조건 점검
        if (inSchedule)
            return true;

        // 2) 스케줄은 아니지만, active = true 면 수동 점검 ON
        if (m.active)
            return true;

        // 3) 그 외에는 점검 아님
        return false;
    }
}
