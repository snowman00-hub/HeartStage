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
    // ================== 1. 시간 / 버전 ==================
    // 마지막 접속 시간 (DateTime.Binary)
    public long lastLoginBinary;

    public DateTime LastLoginTime => DateTime.FromBinary(lastLoginBinary);


    // ================== 2. 인벤토리 / 성장 / 해금 ==================
    // 아이템 인벤토리: 아이템 ID → 수량
    public Dictionary<int, int> itemList = new Dictionary<int, int>();

    // 클리어한 웨이브 ID 목록 (최초 보상 체크용)
    public List<int> clearWaveList = new List<int>();

    // 보유 캐릭터 ID 집합 (중복 방지)
    public List<int> ownedIds = new List<int>();

    // 시스템 해금 여부 (이름 기준)
    public Dictionary<string, bool> unlockedByName = new Dictionary<string, bool>();

    // 경험치를 ID별로 저장
    public Dictionary<int, int> expById = new Dictionary<int, int>();


    // ================== 3. 상점 / 파견 / 자원 ==================
    // 데일리 샵 슬롯 정보 (상점테이블ID, 구매여부 등)
    public List<DailyShopSlot> dailyShopSlotList = new List<DailyShopSlot>();

    // 캐릭터 파견 횟수 (캐릭터ID → 파견 횟수)
    public Dictionary<int, int> characterDispatchCounts = new Dictionary<int, int>();

    // 마지막 파견 리셋 날짜 (예: "2025-12-02")
    public string lastDispatchResetDate = "";


    // ================== 4. 스테이지 / 웨이브 진행 ==================
    public int selectedStageID = -1;
    public int selectedStageStep1 = -1;
    public int selectedStageStep2 = -1;
    public int startingWave = 1;


    // ================== 5. 퀘스트 진행 상태 ==================
    public DailyQuestState dailyQuest = new DailyQuestState();
    public WeeklyQuestState weeklyQuest = new WeeklyQuestState();
    public AchievementQuestState achievementQuest = new AchievementQuestState();


    // ================== 6. 프로필 / 소셜 ==================

    // 닉네임 (""이면 아직 설정 전 → uid로 대체)
    public string nickname = "";

    // 상태 메시지
    public string statusMessage = "";

    // 프로필 아이콘: Addressable assetName (예: "ProfileIcon_Default")
    public string profileIconKey = "";

    // 내가 가진 프로필 아이콘 키 목록
    public List<string> ownedProfileIconKeys = new List<string>();

    // 장착 중인 칭호 ID
    public int equippedTitleId = 0;

    // 팬 수 (프로필/로비에서 표시용)
    public int fanAmount;

    // 보유한 칭호 ID 목록 (TitleTable 기준)
    public List<int> ownedTitleIds = new List<int>();

    // 메인 스테이지 최고 진행도 (예: 3-3)
    public int mainStageStep1 = 0;
    public int mainStageStep2 = 0;

    // 팬미팅(팬 스테이지) 최고 기록 시간 (초)
    public int bestFanMeetingSeconds = 0;

    // 스페셜 스테이지 최고 기록 시간 (초)
    //   - 지금은 공석(0)으로만 유지. UI에서 0이면 "--:--" 같이 비워 보여주기.
    //   - 나중에 스페셜 기록 붙이면 여기 값 갱신해서 쓰면 됨.
    public int specialStageBestSeconds = 0;

    // 드림 에너지
    public int dreamEnergy = 0;
    public int dreamSendDailyLimit = 20;
    public int dreamSendTodayCount = 0;
    public int dreamLastSendDate = 0;


    // ================== 7. 공지 / 기타 ==================
    // 마지막으로 본 공지 ID (1부터 시작)
    public int lastSeenNoticeId = 0;


    // ================== 생성자 / 버전업 ==================
    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        // 나중에 V2로 넘어갈 때 마이그레이션
        throw new NotImplementedException();
    }
}
