using UnityEngine;

public class Stage : MonoBehaviour
{
    public int width = 9;
    public int height = 5;
    public GameObject[] stageCells; // 9x5 = 45 개

    private int[] _floorOwner; // -1 = 없음, >=0 = 캐릭터ID

    private void Awake()
    {
        int len = width * height;
        if (stageCells.Length != len)
            Debug.LogWarning("StageCells 배열 순서 확인 필요");

        _floorOwner = new int[len];
        for (int i = 0; i < len; i++)
            _floorOwner[i] = -1;
    }

    private int Index(int x, int y) => y * width + x;
    private bool InRange(int x, int y) => (x >= 0 && x < width && y >= 0 && y < height);

    public void ApplyPlacementMask(int ownerID, int anchorX, int anchorY, Vector2Int[] offsets)
    {
        foreach (var offset in offsets)
        {
            int x = anchorX + offset.x;
            int y = anchorY + offset.y;
            if (!InRange(x, y)) continue;

            int index = Index(x, y);

            var go = stageCells[index];
            if (go == null) continue;

            var cell = go.GetComponent<StageCell>();
            if (cell == null || !cell.isBuildable) continue;

            _floorOwner[index] = ownerID;
            cell.Refresh(true);
        }
    }

    public void RemovePlacementMask(int ownerID, Vector2Int anchor, Vector2Int[] offsets)
    {
        foreach (var offset in offsets)
        {
            int x = anchor.x + offset.x;
            int y = anchor.y + offset.y;
            if (!InRange(x, y)) continue;

            int index = Index(x, y);
            if (_floorOwner[index] == ownerID)
            {
                _floorOwner[index] = -1;
                stageCells[index].GetComponent<StageCell>().Refresh(false);
            }
        }
    }

    public void MovePlacement(int ownerID, Vector2Int oldAnchor, Vector2Int newAnchor, Vector2Int[] offsets)
    {
        RemovePlacementMask(ownerID, oldAnchor, offsets);
        ApplyPlacementMask(ownerID, newAnchor.x, newAnchor.y, offsets);
    }
}
