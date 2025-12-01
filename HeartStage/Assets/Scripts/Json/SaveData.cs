using System;
using System.Collections.Generic;

[Serializable]
public abstract class SaveData
{
    public int Version { get; protected set; }

    public abstract SaveData VersionUp();
}

[Serializable]
public class SaveDataV1 : SaveData
{
    // 마지막으로 접속했던 시간 (년,월,일,시,분,초) 저장
    // LastLoginTime 프로퍼티로 시간 받기
    public long lastLoginBinary; 

    public Dictionary<int, int> itemList = new Dictionary<int, int>(); // 아이템 ID와 수량을 저장, ItemInvenHelper.cs 사용 추천
    public List<int> clearWaveList = new List<int>(); // 클리어한 웨이브 ID를 저장 -> 최초 보상 체크
    public List<int> ownedIds = new List<int>(); // 보유 캐릭터 아이디 집합 (중복 방지)
    public Dictionary<string, bool> unlockedByName = new Dictionary<string, bool>(); // 해금 여부를 Name별로 저장
    public Dictionary<int, int> expById = new Dictionary<int, int>(); // 경험치를 ID별로 저장
    public List<DailyShopSlot> dailyShopSlotList = new List<DailyShopSlot>(); // 데일리 샵 슬롯 3개 정보 (상점테이블ID,구매여부)

    public DailyQuestState dailyQuest = new DailyQuestState(); // 데일리 퀘스트 진행 상태
    public WeeklyQuestState weeklyQuest = new WeeklyQuestState(); // 위클리 퀘스트 진행 상태
    public AchievementQuestState achievementQuest = new AchievementQuestState(); // 업적 퀘스트 진행 상태

    public int selectedStageID = -1;
    public int selectedStageStep1 = -1;
    public int selectedStageStep2 = -1;
    public int startingWave = 1;

    public int fanAmount; // 팬 수
    public Dictionary<int, int> characterDispatchCounts = new Dictionary<int, int>(); // 파견 중인 캐릭터 ID와 파견 횟수
    public string lastDispatchResetDate = ""; // 마지막 파견 리셋 날짜 

    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        throw new NotImplementedException();
    }

    // FirebaseTime.GetServerTime()과 비교해서 시간 이벤트 주기
    public DateTime LastLoginTime => DateTime.FromBinary(lastLoginBinary);
}