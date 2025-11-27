using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using UnityEditor.Overlays;


#if UNITY_EDITOR
using UnityEditor;
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

#if UNITY_EDITOR
        string lastSceneStr = EditorPrefs.GetString(LastSceneKey, string.Empty);
        if (!string.IsNullOrEmpty(lastSceneStr) &&
            System.Enum.TryParse(lastSceneStr, out SceneType lastSceneType))
        {
            // 부트/타이틀 같은 애들은 스킵하고 싶으면 여기서 필터
            if (lastSceneType != SceneType.None &&
                lastSceneType != SceneType.TitleScene)
            {
                targetScene = lastSceneType;
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
        // 이제는 string이 아니라 SceneType으로 호출
        await GameSceneManager.ChangeScene(targetScene);
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