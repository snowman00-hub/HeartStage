using UnityEngine;

public class Test2Window : GenericWindow
{
    public override void Open()
    {
        base.Open();
        Debug.Log("Test2Window Opened");
    }

    public override void Close()
    {
        base.Close();
        Debug.Log("Test2Window Closed");
    }
}

