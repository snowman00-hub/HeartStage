using UnityEngine;

public class QuestTypeData
{
    public int Quest_type { get; set; }
    public string Quest_type_name { get; set; }
    public bool Quest_reset { get; set; }
    public string Quest_reset_time { get; set; }
}


public enum QuestType
{
    Achievement = 0,
    Daily = 1,
    Weekly = 2,
    Event = 3,
}