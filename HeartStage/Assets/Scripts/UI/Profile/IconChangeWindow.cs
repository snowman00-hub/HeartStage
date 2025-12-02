using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IconChangeWindow : MonoBehaviour
{
    public static IconChangeWindow Instance;

    [Header("루트")]
    [SerializeField] private GameObject root;          // 패널 루트 (모달 패널의 자식)

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;    // ScrollView Content
    [SerializeField] private GameObject iconItemPrefab; // 비활성 프리팹 (Image + Button [+ TMP_Text])

    public bool IsOpen => root != null && root.activeSelf;

    private readonly List<GameObject> _spawnedItems = new();

    private void Awake()
    {
        Instance = this;
        if (root != null)
            root.SetActive(false);
    }

    public void Open()
    {
        if (root == null || contentRoot == null || iconItemPrefab == null)
        {
            Debug.LogWarning("[IconChangeWindow] 세팅이 안 되어 있음");
            return;
        }

        root.SetActive(true);
        RebuildList();
    }

    // 모달 패널에서 직접 호출할 내부 Close
    public void CloseInternal()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void CloseWithModal()
    {
        CloseInternal();
        ProfileWindow.Instance?.HideModalPanel();
    }

    private void RebuildList()
    {
        // 기존 아이템 정리
        foreach (var go in _spawnedItems)
        {
            if (go != null)
                Destroy(go);
        }
        _spawnedItems.Clear();

        var data = SaveLoadManager.Data as SaveDataV1;
        if (data == null)
        {
            Debug.LogWarning("[IconChangeWindow] SaveDataV1 없음");
            return;
        }

        var charTable = DataTableManager.CharacterTable;
        if (charTable == null)
        {
            Debug.LogWarning("[IconChangeWindow] CharacterTable 없음");
            return;
        }

        var unlocked = data.unlockedByName;
        if (unlocked == null || unlocked.Count == 0)
        {
            Debug.LogWarning("[IconChangeWindow] unlockedByName 비어있음");
        }

        // 🔹 해금 기준(unlockedByName == true)으로 아이콘 키 수집
        HashSet<string> iconKeys = new();

        if (unlocked != null)
        {
            foreach (var kv in unlocked)
            {
                string charName = kv.Key;
                bool isUnlocked = kv.Value;

                if (!isUnlocked)
                    continue;

                var row = charTable.GetByName(charName); // 이름으로 캐릭터 찾기 :contentReference[oaicite:2]{index=2}
                if (row == null)
                {
                    continue;
                }

                string iconKey = row.icon_imageName;
                if (string.IsNullOrEmpty(iconKey))
                    continue;

                // 실제 스프라이트 있는 것만 사용
                var sprite = ResourceManager.Instance.GetSprite(iconKey);
                if (sprite == null)
                {
                    continue;
                }

                iconKeys.Add(iconKey);
            }
        }

        // (선택) 이벤트/보상 아이콘도 끼우고 싶으면 ownedProfileIconKeys도 합침
        if (data.ownedProfileIconKeys != null)
        {
            foreach (var key in data.ownedProfileIconKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                var sprite = ResourceManager.Instance.GetSprite(key);
                if (sprite == null)
                    continue;

                iconKeys.Add(key);
            }
        }

        // 정말 아무 것도 없으면 fallback 하나 넣기
        if (iconKeys.Count == 0)
        {
            const string fallback = "hanaicon";
            var fallbackSprite = ResourceManager.Instance.GetSprite(fallback);
            if (fallbackSprite != null)
            {
                iconKeys.Add(fallback);
            }
        }

        // UI 아이템 생성
        foreach (var key in iconKeys)
        {
            var sprite = ResourceManager.Instance.GetSprite(key);
            if (sprite == null)
                continue;

            var go = Instantiate(iconItemPrefab, contentRoot);
            go.SetActive(true);
            _spawnedItems.Add(go);

            var image = go.GetComponentInChildren<Image>();
            if (image != null)
                image.sprite = sprite;

            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = key;

            var btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string capturedKey = key;
                btn.onClick.AddListener(() => OnClickIcon(capturedKey).Forget());
            }
        }
    }

    private async UniTaskVoid OnClickIcon(string key)
    {
        var data = SaveLoadManager.Data as SaveDataV1;
        if (data == null)
            return;

        data.profileIconKey = key;

        if (!data.ownedProfileIconKeys.Contains(key))
            data.ownedProfileIconKeys.Add(key);

        await SaveLoadManager.SaveToServer();

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);
        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);

        ProfileWindow.Instance?.RefreshAll();

        CloseWithModal();
    }
}
