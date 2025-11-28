using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Linq;
#endif

public class BootStrap : MonoBehaviour
{
    private const string LastSceneKey = "LastSceneType";
    public static bool IsInitialized = false;
    // Scene 새로 복사했으면 Addressable 체크하고 플레이 하기, 등록 안되면 오류 뜸!
    // Scene Addressable 주소 바꾸지 말기, 그냥 체크만 하기
    private async UniTask Start()
    {
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

        SceneType targetScene = SceneType.LobbyScene;
        string targetAddress = null;

#if UNITY_EDITOR
        string lastScenePath = string.Empty;
        try
        {
            // 먼저 전역 타입 검색
            var type = Type.GetType("EditPlayScene");
            if (type == null)
            {
                // 어셈블리들에서 이름으로 검색
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    try
                    {
                        var t = asm.GetTypes().FirstOrDefault(x => x.Name == "EditPlayScene");
                        if (t != null)
                        {
                            type = t;
                            break;
                        }
                    }
                    catch { /* 일부 어셈블리에서 예외 발생 가능, 무시 */ }
                }
            }

            if (type != null)
            {
                var method = type.GetMethod("GetLastScene", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    var result = method.Invoke(null, null) as string;
                    lastScenePath = result ?? string.Empty;
                }
                else
                {
                    Debug.LogWarning("EditPlayScene 타입은 찾았으나 GetLastScene 메서드를 찾지 못했습니다.");
                }
            }
            else
            {
                Debug.LogWarning("EditPlayScene 타입을 찾지 못했습니다. Editor 유틸이 올바르게 위치했는지 확인하세요.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"EditPlayScene.GetLastScene() 호출 중 예외 발생: {ex.Message}");
            lastScenePath = string.Empty;
        }

        if (!string.IsNullOrEmpty(lastScenePath))
        {
            // 파일명(확장자 제외) 추출: 예) "Assets/Scenes/StageScene.unity" -> "StageScene"
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(lastScenePath);

            // 부트/타이틀 같은 씬은 스킵
            if (!string.IsNullOrEmpty(sceneName) &&
                sceneName != "BootScene" &&
                sceneName != "TitleScene")
            {
                // Addressables 설정에서 해당 씬의 주소(address)를 찾아본다.
                // 실패하면 씬 이름(sceneName)을 fallback으로 사용
                var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
                if (settings != null)
                {
                    string guid = UnityEditor.AssetDatabase.AssetPathToGUID(lastScenePath);
                    var entry = settings.FindAssetEntry(guid);
                    if (entry != null && !string.IsNullOrEmpty(entry.address))
                    {
                        targetAddress = entry.address;
                    }
                    else
                    {
                        // Addressables에 등록되어있지 않으면 씬 이름을 시도 값으로 둠
                        targetAddress = sceneName;
                    }
                }
                else
                {
                    targetAddress = sceneName;
                }
            }
        }
#else
        Application.targetFrameRate = 60;
#endif
        // Firebase 로그인 될때까지 대기
        await UniTask.WaitUntil(()=> AuthManager.Instance.IsLoggedIn);
        // 서버 시간 가져오는 클래스 초기화, Firebase Initialization 이후에 실행하기
        FirebaseTime.Initialize(); 
        // 서버에서 데이터 로드
        await TryLoad();
        // 유저가 마지막으로 접속한 시간 갱신, 로그인 보상 주기
        await UpdateLastLoginTime();

        // 에디터에서 주소를 찾았으면 Addressables 주소 기반으로 씬 로드
        if (!string.IsNullOrEmpty(targetAddress))
        {
            await Addressables.LoadSceneAsync(targetAddress);
        }
        else
        {
            // 기존 동작 유지: SceneType 기반으로 매핑된 Addressables 주소로 전환
            await GameSceneManager.ChangeScene(targetScene);
        }
    }

    private async UniTask UpdateLastLoginTime()
    {
        DateTime now = FirebaseTime.GetServerTime();
        DateTime last = SaveLoadManager.Data.LastLoginTime;

        // 시간에 따라 이벤트 처리하기
        if (last.Date != now.Date) //
        {
            // ex) 일일 로그인 보상 주기

            // 출석 퀘스트 처리
            QuestManager.Instance.OnAttendance();
        }

        // 이벤트 처리 후, 현재 시간으로 마지막 접속 시간 업데이트 // 나중에 앱 종료할때도 해야할듯?
        SaveLoadManager.Data.lastLoginBinary = FirebaseTime.GetServerTime().ToBinary();
        await SaveLoadManager.SaveToServer();
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

            // 처음에 드림에너지 100개 주기
            ItemInvenHelper.AddItem(ItemID.DreamEnergy, 100);
            // 첫 저장은 await로 확실하게 보장하는 게 좋다

            // 처음에 출석 퀘스트 처리
            QuestManager.Instance.OnAttendance();

            await SaveLoadManager.SaveToServer();
        }
    }
}