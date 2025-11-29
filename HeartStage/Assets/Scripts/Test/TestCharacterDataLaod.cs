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

        // 기본 정렬: 이름 → id
        SortByNameInternal();
        ReportTestProgress(0.5f);

        // 2) 캐릭 프리팹 Instantiate + 패널 InitAsync 전부 기다리기
        await InstantiateCharactersAsync();
        ReportTestProgress(0.9f);

        // Addressables.Release(handle); // 필요시

        IsReady = true;
        ReportTestProgress(1.0f);
    }

    /// <summary>
    /// 현재 characters 리스트 기준으로 스크롤뷰를 다시 그린다.
    /// (정렬/필터 후에 이 함수를 불러주면 됨)
    /// </summary>
    public async UniTask RebuildAsync()
    {
        await InstantiateCharactersAsync();
    }

    /// <summary>
    /// 이름순 정렬 (이름, 같으면 id)
    /// UI 버튼에서 OnClick에 연결해서 써도 됨.
    /// </summary>
    public async void ApplySortByName()
    {
        SortByNameInternal();
        await InstantiateCharactersAsync();
    }

    private void SortByNameInternal()
    {
        characters.Sort((a, b) =>
        {
            int cmp = a.char_name.CompareTo(b.char_name);
            if (cmp != 0) return cmp;
            return a.char_id.CompareTo(b.char_id);
        });
    }

    /// <summary>
    /// 랭크순 정렬 (낮은 랭크 → 높은 랭크, 같으면 id)
    /// </summary>
    public async void ApplySortByRank()
    {
        characters.Sort((a, b) =>
        {
            int cmp = a.char_rank.CompareTo(b.char_rank);
            if (cmp != 0) return cmp;
            return a.char_id.CompareTo(b.char_id);
        });

        await InstantiateCharactersAsync();
    }

    /// <summary>
    /// 내부에서만 사용하는 실제 Instantiate 로직.
    /// characters 리스트 전체를 기준으로 다시 만든다.
    /// </summary>
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
