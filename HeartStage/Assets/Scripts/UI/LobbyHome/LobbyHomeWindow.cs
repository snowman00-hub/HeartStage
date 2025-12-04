using UnityEngine;

public class LobbyHomeWindow : GenericWindow
{
    public LobbyHomeInitializer initializer;

    private void OnEnable()
    {
        initializer.Init();
    }
}