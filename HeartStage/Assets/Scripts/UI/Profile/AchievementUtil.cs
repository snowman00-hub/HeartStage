public static class AchievementUtil
{
    public static int GetCompletedAchievementCount(SaveDataV1 data)
    {
        if (data == null || data.achievementQuest == null)
            return 0;

        // 여기 부분은 네 AchievementQuestState 구조에 맞게 조정
        // 예: completedQuestIds, completedList 등
        var state = data.achievementQuest;

        if (state.completedQuestIds != null)
            return state.completedQuestIds.Count;

        return 0;
    }
}

