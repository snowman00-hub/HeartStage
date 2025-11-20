using UnityEngine;

public class WorldGridLayout : MonoBehaviour
{
    public int columns = 5;                // 한 줄에 몇 개?

    public Vector2 cellSize = new Vector2(1f, 1f);        // x = 가로 크기, y = 세로 크기
    public Vector2 cellSpacing = new Vector2(0.2f, 0.2f); // x = 가로 간격, y = 세로 간격

    private void OnValidate()
    {
        Arrange();
    }

    public void Arrange()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 pos = new Vector3(
                col * (cellSize.x + cellSpacing.x),
                -row * (cellSize.y + cellSpacing.y),
                0
            );

            transform.GetChild(i).localPosition = pos;
        }
    }
}
