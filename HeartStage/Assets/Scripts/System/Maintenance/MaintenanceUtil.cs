using System;

public static class MaintenanceUtil
{
    public static bool IsMaintenanceNow(MaintenanceData m, DateTimeOffset now)
    {
        if (m == null)
            return false;

        // 1) 운영자가 "active = true"로 강제 점검 켜놓은 경우 → 무조건 점검
        if (m.active)
            return true;

        // 2) 시간 기반 점검 (startAt ~ endAt 사이면 점검)
        if (!string.IsNullOrEmpty(m.startAt))
        {
            if (DateTimeOffset.TryParse(m.startAt, out var start))
            {
                // endAt이 비어 있으면 "start 이후 계속"으로 봐도 되고,
                // 있으면 그 사이까지만
                if (string.IsNullOrEmpty(m.endAt))
                {
                    // now >= start 이면 점검
                    if (now >= start)
                        return true;
                }
                else if (DateTimeOffset.TryParse(m.endAt, out var end))
                {
                    // start <= now <= end 이면 점검
                    if (now >= start && now <= end)
                        return true;
                }
            }
        }

        return false;
    }
}
