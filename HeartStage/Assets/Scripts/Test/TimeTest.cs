using UnityEngine;

public class TimeTest : MonoBehaviour
{
	[ContextMenu("타임 복구")]
	public void TimeRestor()
	{
		Time.timeScale = 1.0f;
	}
}
