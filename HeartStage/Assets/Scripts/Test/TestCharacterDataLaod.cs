using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestCharacterDataLaod : MonoBehaviour
{
    // Addressables에서 불러온 CharacterData SO들
    private readonly List<CharacterData> characters = new List<CharacterData>();

    [Header("UI 부모 (Scroll View의 Content)")]
    public RectTransform content;     // ScrollView → Viewport → Content
    public DragMe CharacterPrefab;    // DragMe 달려있는 공통 프리팹

    // Start를 async로 써서 Addressables 기다리게 만들자
    private async void Start()
    {
        // 1) 라벨 "CharacterData" 달린 SO들 전부 로드
        AsyncOperationHandle<IList<CharacterData>> handle =
            Addressables.LoadAssetsAsync<CharacterData>(
                "CharacterData",  // 네가 붙인 라벨 이름 (다르면 여기만 바꾸면 됨)
                null              // 개별 로드 콜백 필요 없으면 null
            );

        IList<CharacterData> loadedList = await handle.Task;

        characters.Clear();
        characters.AddRange(loadedList);

        Debug.Log($"[CharacterDataLoad] Loaded {characters.Count} CharacterData SOs.");

        characters.Sort((a, b) =>
        {
            // 1차: 이름 기준
            int cmp = a.char_name.CompareTo(b.char_name);
            if (cmp != 0)
                return cmp;

            // 2차: id 기준
            return a.char_id.CompareTo(b.char_id);
        });


        // 3) UI에 깔기
        InstantiateCharacters();

        // 필요하면 나중에 Release (여기선 안 해도 큰 문제는 없음)
        // Addressables.Release(handle);
    }

    private void InstantiateCharacters()
    {
        foreach (var characterData in characters)
        {
            // 공통 DragMe 프리팹을 Content 밑에 생성
            DragMe dragMeInstance = Instantiate(CharacterPrefab, content);
            dragMeInstance.name = characterData.name;  // or characterData.ID.ToString()

            // DragMe 쪽에 SO 꽂아주기
            // 네 DragMe에 RebindCharacter가 있으면 이거 쓰는 걸 추천
            if (dragMeInstance != null)
            {
                // 1) 레지스트리까지 갱신하고 싶으면:
                //dragMeInstance.RebindCharacter(characterData);

                // 2) 단순 바인딩만이면:
                dragMeInstance.characterData = characterData;
            }

            // RectTransform 초기화 (LayoutGroup이 위치 정리하게)
            if (dragMeInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }
    }
}

