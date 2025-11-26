using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class TestCharacterDataLaod : MonoBehaviour
{
    private readonly List<CharacterData> characters = new List<CharacterData>();

    public RectTransform content;
    public DragMe CharacterPrefab;

    public bool IsReady { get; private set; }

    // 이 컴포넌트가 담당할 전역 프로그레스 구간 (예: 60% ~ 85%)
    private const float GlobalStart = 0.6f;
    private const float GlobalEnd = 0.85f;

    private void ReportTestProgress(float local01)
    {
        float clamped = Mathf.Clamp01(local01);
        float global = Mathf.Lerp(GlobalStart, GlobalEnd, clamped);
        SceneLoader.SetProgressExternal(global);
    }

    private async void Start()
    {
        IsReady = false;
        ReportTestProgress(0.0f);

        // 1) Addressables 로드
        var handle =
            Addressables.LoadAssetsAsync<CharacterData>(
                "CharacterData",
                null
            );

        IList<CharacterData> loadedList = await handle.Task;
        ReportTestProgress(0.3f);

        characters.Clear();
        characters.AddRange(loadedList);

        // 정렬 등등 네가 하던 거
        characters.Sort((a, b) =>
        {
            int cmp = a.char_name.CompareTo(b.char_name);
            if (cmp != 0) return cmp;
            return a.char_id.CompareTo(b.char_id);
        });
        ReportTestProgress(0.5f);

        // 2) 캐릭 프리팹 Instantiate + 패널 InitAsync 전부 기다리기
        await InstantiateCharactersAsync();
        ReportTestProgress(0.9f);

        // Addressables.Release(handle); // 필요시

        IsReady = true;
        ReportTestProgress(1.0f);
    }

    private async UniTask InstantiateCharactersAsync()
    {
        if (content == null || CharacterPrefab == null)
        {
            Debug.LogWarning("[TestCharacterDataLaod] content 또는 CharacterPrefab 미할당");
            return;
        }

        // 기존 자식 삭제
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        var initTasks = new List<UniTask>();

        foreach (var characterData in characters)
        {
            var dragMeInstance = Instantiate(CharacterPrefab, content);
            dragMeInstance.name = characterData.char_name;
            dragMeInstance.characterData = characterData;

            var panel = dragMeInstance.GetComponent<CharacterSelectTestPanel>();
            if (panel != null)
            {
                // 🔹 InitAsync의 UniTask를 모아둔다
                initTasks.Add(panel.InitAsync(characterData));
            }

            // RectTransform 정리
            if (dragMeInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }

        // 🔹 모든 패널의 InitAsync(텍스트 + 카드 이미지 로드)가 끝날 때까지 기다린다
        if (initTasks.Count > 0)
            await UniTask.WhenAll(initTasks);
    }
}
