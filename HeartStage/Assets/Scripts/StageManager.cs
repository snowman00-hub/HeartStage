using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GoLobby()
    {
        LoadSceneManager.Instance.GoLobby();
    }
}