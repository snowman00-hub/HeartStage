using UnityEngine;

public class QuestProgressData
{
    // CSV columns (정확한 네이밍 유지):
    // progress_reward_ID	progress_type	progress_amount	reward1	reward1_amount	reward2	reward2_amount	reward3	reward3_amount	Notfill_icon	filled_icon	get_reward_icon

    public int progress_reward_ID { get; set; }
    public ProgressType progress_type { get; set; }
    public int progress_amount { get; set; }

    public int reward1 { get; set; }
    public int reward1_amount { get; set; }
    public int reward2 { get; set; }
    public int reward2_amount { get; set; }
    public int reward3 { get; set; }
    public int reward3_amount { get; set; }

    // 아이콘은 리소스 키나 경로를 저장하는 문자열로 처리
    public string Notfill_icon { get; set; }
    public string filled_icon { get; set; }
    public string get_reward_icon { get; set; }
}

public enum ProgressType
{
    Achievement = 0,
    Daily = 1,
    Weekly = 2,
    Event = 3,
}