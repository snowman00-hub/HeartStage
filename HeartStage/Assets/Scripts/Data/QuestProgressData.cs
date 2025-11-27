using UnityEngine;

public class QuestProgressData
{
    // CSV columns:
    // progress_reward_ID	progress_type	progress_amount	reward1	reward1_amount	reward2	reward2_amount	reward3	reward3_amount	Notfill_icon	filled_icon	get_reward_icon

    public int Progress_Reward_ID { get; set; }
    public int Progress_Type { get; set; }      // CSV 값은 int로 취급. 필요하면 enum으로 교체하세요.
    public int Progress_Amount { get; set; }

    public int Reward1 { get; set; }
    public int Reward1_Amount { get; set; }
    public int Reward2 { get; set; }
    public int Reward2_Amount { get; set; }
    public int Reward3 { get; set; }
    public int Reward3_Amount { get; set; }

    // 아이콘은 리소스 키나 경로를 저장하는 문자열로 처리
    public string NotFill_Icon { get; set; }
    public string Filled_Icon { get; set; }
    public string Get_Reward_Icon { get; set; }
}

public enum ProgressType
{
    Achievement = 0,
    Daily = 1,
    Weekly = 2,
    Event = 3,
}