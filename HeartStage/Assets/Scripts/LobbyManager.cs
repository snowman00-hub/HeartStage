using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;

    public TextMeshProUGUI lightStickCountText;
    public TextMeshProUGUI heartStickCountText;

    private int lightStickCount = 0;
    public int LightStickCount
    {
        get { return lightStickCount; }
        set
        {
            lightStickCount = value;
            lightStickCountText.text = $"{lightStickCount}";
        }
    }

    private int heartStickCount = 0;
    public int HeartStickCount
    {
        get { return heartStickCount; }
        set
        {
            heartStickCount = value;
            heartStickCountText.text = $"{heartStickCount}";
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MoneyUISet();
    }

    public void GoStage()
    {
        LoadSceneManager.Instance.GoStage();
    }

    private void MoneyUISet()
    {
        var itemList = SaveLoadManager.Data.itemList;
        if (itemList.ContainsKey(ItemID.LightStick))
        {
            LightStickCount = itemList[ItemID.LightStick];
        }
        else
        {
            LightStickCount = 0;
        }

        if (itemList.ContainsKey(ItemID.HeartStick))
        {
            HeartStickCount = itemList[ItemID.HeartStick];
        }
        else
        {
            HeartStickCount = 0;
        }
    }
}