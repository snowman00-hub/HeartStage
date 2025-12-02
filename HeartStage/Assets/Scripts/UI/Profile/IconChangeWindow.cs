using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconChangeWindow : MonoBehaviour
{
    public static IconChangeWindow Instance;

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject iconItemPrefab;

    public bool IsOpen => gameObject.activeSelf;

    private readonly List<GameObject> _spawnedItems = new();

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RebuildList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ProfileWindow.Instance?.OnPopupClosed();
    }

    public void Prewarm()
    {
        bool wasActive = gameObject.activeSelf;
        gameObject.SetActive(true);
        RebuildList();
        gameObject.SetActive(wasActive);
    }

    private void RebuildList()
    {
        foreach (var go in _spawnedItems)
        {
            if (go != null)
                Destroy(go);
        }
        _spawnedItems.Clear();

        var data = SaveLoadManager.Data as SaveDataV1;
        if (data == null)
            return;

        var charTable = DataTableManager.CharacterTable;
        if (charTable == null)
            return;

        var unlocked = data.unlockedByName;

        HashSet<string> iconKeys = new();

        if (unlocked != null)
        {
            foreach (var kv in unlocked)
            {
                string charName = kv.Key;
                bool isUnlocked = kv.Value;

                if (!isUnlocked)
                    continue;

                var row = charTable.GetByName(charName);
                if (row == null)
                    continue;

                string iconKey = row.icon_imageName;
                if (string.IsNullOrEmpty(iconKey))
                    continue;

                var sprite = ResourceManager.Instance.GetSprite(iconKey);
                if (sprite == null)
                    continue;

                iconKeys.Add(iconKey);
            }
        }

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

        if (iconKeys.Count == 0)
        {
            const string fallback = "hanaicon";
            var fallbackSprite = ResourceManager.Instance.GetSprite(fallback);
            if (fallbackSprite != null)
                iconKeys.Add(fallback);
        }

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

        Close();
    }
}
