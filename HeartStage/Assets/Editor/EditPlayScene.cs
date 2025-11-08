using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class EditPlayScene
{
    private const string Key = "LastPlayedScenePath";

    static EditPlayScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // 플레이 버튼 눌렸을때
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 현재 열린 씬 저장
            string currentScene = EditorSceneManager.GetActiveScene().path;
            EditorPrefs.SetString(Key, currentScene);

            // Bootstrap 씬 강제 설정
            var bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/BootScene.unity"); 
            EditorSceneManager.playModeStartScene = bootstrapScene;
        }
    }

    public static string GetLastScene()
    {
        return EditorPrefs.GetString(Key, "");
    }
}
