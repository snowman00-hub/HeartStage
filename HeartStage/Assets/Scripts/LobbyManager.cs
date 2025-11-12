using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GoStage()
    {
        LoadSceneManager.Instance.GoStage();
    }
}