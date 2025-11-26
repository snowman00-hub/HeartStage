using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootStrap : MonoBehaviour
{
    public static bool IsInitialized = false;

    private const string Key = "LastPlayedScenePath";

    // Scene 새로 복사했으면 Addressable 체크하고 플레이 하기, 등록 안되면 오류 뜸!
    // Scene Addressable 주소 바꾸지 말기, 그냥 체크만 하기
    private async UniTask Start()
    {
        // 처음 한번만 로드하기, 로그 아웃시 다시 BootScene으로 돌아옴
        if (!IsInitialized)
        {
            // Addressables 초기화
            await Addressables.InitializeAsync();
            // 비동기로 미리 해야하는 작업들 있으면 가능한 부트 씬에서 하고 해당 씬에선 동기로 쓰기
            await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);
            await ResourceManager.Instance.PreloadLabelAsync("SFX"); // 사운드 추가 로드
            //await ResourceManager.Instance.PreloadLabelAsync("BGM");
            await DataTableManager.Initialization;

            IsInitialized = true;
        }

        string targetScene = "Assets/Scenes/Lobby.unity";

#if UNITY_EDITOR
        string lastScene = EditorPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(lastScene) && lastScene != "Assets/Scenes/bootScene.unity")
            targetScene = lastScene;
#else
        Application.targetFrameRate = 60;
#endif
        // Firebase 로그인 될때까지 대기
        await UniTask.WaitUntil(()=> AuthManager.Instance.IsLoggedIn);
        // 서버에서 데이터 로드
        await TryLoad();

        await Addressables.LoadSceneAsync(targetScene);
    }

    private async UniTask TryLoad()
    {
        bool loaded = await SaveLoadManager.LoadFromServer();

        if (!loaded)
        {
            // 기본 세이브 생성
            var charTable = DataTableManager.CharacterTable;

            charTable.BuildDefaultSaveDictionaries(
                new[] { "하나" },
                out var unlockedByName,
                out var expById,
                out var ownedBaseIds
            );

            SaveLoadManager.Data.unlockedByName = unlockedByName;
            SaveLoadManager.Data.expById = expById;

            foreach (var id in ownedBaseIds)
                SaveLoadManager.Data.ownedIds.Add(id);

            // 첫 저장은 await로 확실하게 보장하는 게 좋다
            await SaveLoadManager.SaveToServer();
        }
    }
}