public static class AchievementUtil
{
    public static int GetCompletedAchievementCount(SaveDataV1 data)
    {
        if (data == null || data.achievementQuest == null)
            return 0;

        // achievementQuest 안 구조에 맞게 수정
        // 예: completedQuestIds 리스트가 있다면:
        var state = data.achievementQuest;
        if (state.completedQuestIds != null)
            return state.completedQuestIds.Count;

        return 0;
    }
}
