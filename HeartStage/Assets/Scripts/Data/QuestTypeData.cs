using UnityEngine;

public class QuestTypeData
{
    public int Quest_Type { get; set; }
    public string Quest_Type_Name { get; set; }
    public int Quest_Reset { get; set; }
    public string Quest_Reset_Time { get; set; }
}


public enum QuestType
{
    Achievement = 0,
    Daily = 1,
    Weekly = 2,
    Event = 3,
}