using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootStrap : MonoBehaviour
{
    private const string Key = "LastPlayedScenePath";

    // Scene 새로 복사했으면 Addressable 체크하고 플레이 하기, 등록 안되면 오류 뜸!
    // Scene Addressable 주소 바꾸지 말기, 그냥 체크만 하기
    private async UniTask Start()
    {
        // Addressables 초기화
        await Addressables.InitializeAsync();
        // 비동기로 미리 해야하는 작업들 있으면 가능한 부트 씬에서 하고 해당 씬에선 동기로 쓰기
        await ResourceManager.Instance.PreloadLabelAsync(AddressableLabel.Stage);
        await ResourceManager.Instance.PreloadLabelAsync("SFX"); // 사운드 추가 로드
        //await ResourceManager.Instance.PreloadLabelAsync("BGM");
        await DataTableManager.Initialization;


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

        // 세이브 데이터 로드
        TryLoad();

        await Addressables.LoadSceneAsync(targetScene);
    }

    private void TryLoad()
    {
        if (!SaveLoadManager.Load())
        {
            var charTable = DataTableManager.CharacterTable;

            charTable.BuildDefaultSaveDictionaries(
                new[] { "하나" },                   // 스타터 이름만 여기
                out var unlockedByName,
                out var expById,
                out var ownedBaseIds
            );

            SaveLoadManager.Data.unlockedByName = unlockedByName;
            SaveLoadManager.Data.expById = expById;

            // 네가 current id 리스트/딕셔너리 어디에 들고있냐에 맞춰서
            foreach (var id in ownedBaseIds)
                SaveLoadManager.Data.ownedIds.Add(id); // List<int>면 이렇게

            SaveLoadManager.Save();
        }
    }
}