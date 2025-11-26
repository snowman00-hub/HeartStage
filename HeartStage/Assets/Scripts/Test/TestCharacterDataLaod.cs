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
        if (content == null || CharacterPrefab == null)
        {
            Debug.LogWarning("[OwnedCharacterSetup] content 또는 CharacterPrefab 미할당");
            return;
        }

        // 기존 자식들 정리 (리프레시 가능하게)
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        foreach (var characterData in characters)
        {
            var dragMeInstance = Instantiate(CharacterPrefab, content);
            dragMeInstance.name = characterData.char_name;

            // 1) DragMe에 데이터 꽂고
            dragMeInstance.characterData = characterData;

            // 2) CharacterSelectPanel도 바로 초기화
            var panel = dragMeInstance.GetComponent<CharacterSelectPanel>();
            if (panel != null)
                panel.Init(characterData);

            // 3) RectTransform 정리
            if (dragMeInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }
    }
}

