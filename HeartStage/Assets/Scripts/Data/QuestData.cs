using UnityEngine;

public class QuestData
{
    public int Quest_ID { get; set; }
    public QuestType Quest_Type { get; set; }
    public string Quest_Info { get; set; }
    public int Quest_Reward1 { get; set; }
    public int Quest_Reward1_A { get; set; }
    public int Quest_Reward2 { get; set; }
    public int Quest_Reward2_A { get; set; }
    public int Quest_Reward3 { get; set; }
    public int Quest_Reward3_A { get; set; }
    public int Progress_Type { get; set; }
    public int Progress_Amount { get; set; }
    public string Icon_Image { get; set; }
}