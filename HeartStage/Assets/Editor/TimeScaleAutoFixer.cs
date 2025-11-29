using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TimeScaleAutoFixer
{
    private static double lastTime;

    static TimeScaleAutoFixer()
    {
        lastTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        // 플레이 중일 때는 건드리지 않음
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        double now = EditorApplication.timeSinceStartup;

        // 5초마다 수행
        if (now - lastTime > 5.0)
        {
            lastTime = now;

            if (Time.timeScale != 1f)
            {
                Debug.Log($"[TimeScaleAutoFixer] (Editor Only) TimeScale 복구: {Time.timeScale} → 1");
                Time.timeScale = 1f;
            }
        }
    }
}
