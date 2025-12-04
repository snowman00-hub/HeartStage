using UnityEngine;

public class LobbyHomeInitializer : MonoBehaviour
{
    public GameObject characterBase; // 캐릭터 이미지 프리팹의 부모가 될 베이스 프리팹

    public void Init()
    {
        // 이전꺼 삭제
        var prevObjects = GameObject.FindGameObjectsWithTag(Tag.LobbyHomeObject);
        foreach (var obj in prevObjects)
        {
            Destroy(obj);
        }

        var ownCharacterIds = SaveLoadManager.Data.ownedIds;
        // 배경 바운더리
        Bounds bounds = DragZoomPanManager.Instance.InnerBounds;

        foreach (var characterId in ownCharacterIds)
        {
            var characterData = DataTableManager.CharacterTable.Get(characterId);
            var imagePrefab = ResourceManager.Instance.Get<GameObject>(characterData.image_PrefabName);

            var go = Instantiate(characterBase, transform);
            Instantiate(imagePrefab, go.transform);

            // 랜덤 위치에서 등장
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            go.transform.position = new Vector3(x, y, 0f);
        }
    }
}