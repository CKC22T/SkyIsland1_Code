#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneJumper : EditorWindow
{
    private static readonly string mSceneReletivePath = "Assets/CKC2022/Scenes/";

    // Scene folder names
    private static readonly string mFinalFolderName = "Final";

    // Unity scene names
    private static readonly string mLoginScene = "Login";
    private static readonly string mTitleScene = "Title";
    private static readonly string mLobbyScene = "Lobby";
    private static readonly string mClientScene = "Client";
    private static readonly string mServerScene = "Server";
    private static readonly string mFinalStageScene = "FinalStage";
    private static readonly string mCreditScene = "Credit";

    // Priority group index
    private const int mSceneChangePriorityGroup = 10;
    private const int mPlayPriorityGroup = 50;

    private const int mLegacySceneChangePriorityGroup = 110;
    private const int mLegacyGameMapScenePriorityGroup = 150;
    private const int mLegacyPlayPriorityGroup = 200;

    // Change unity scene
    [MenuItem("Jumper/Login Scene", priority = mSceneChangePriorityGroup)] private static void changeSceneToLoginScene() => changeScene(getScenePath(SceneFolderType.Final, mLoginScene));
    [MenuItem("Jumper/Title Scene", priority = mSceneChangePriorityGroup)] private static void changeSceneToTitleScene() => changeScene(getScenePath(SceneFolderType.Final, mTitleScene));
    [MenuItem("Jumper/Lobby Scene", priority = mSceneChangePriorityGroup)] private static void changeSceneToLobbyScene() => changeScene(getScenePath(SceneFolderType.Final, mLobbyScene));
    [MenuItem("Jumper/Client Scene", priority = mSceneChangePriorityGroup)] private static void changeSceneToClientScene() => changeScene(getScenePath(SceneFolderType.Final, mClientScene));
    [MenuItem("Jumper/Server Scene", priority = mSceneChangePriorityGroup)] private static void changeSceneToServerScene() => changeScene(getScenePath(SceneFolderType.Final, mServerScene));
    [MenuItem("Jumper/Final Stage Scene", priority = mSceneChangePriorityGroup)] public static void changeSceneToFinalStage() => changeScene(getScenePath(SceneFolderType.Final, mFinalStageScene));
    [MenuItem("Jumper/Credit Scene", priority = mSceneChangePriorityGroup)] public static void changeSceneToCredit() => changeScene(getScenePath(SceneFolderType.Final, mCreditScene));

    // Instance play unity scene
    [MenuItem("Jumper/Play Login Scene", priority = mPlayPriorityGroup)] public static void playLoginScene() => playScene(getScenePath(SceneFolderType.Final, mLoginScene));
    //[MenuItem("Jumper/Play Title Scene", priority = mPlayPriorityGroup)] public static void playTitleScene() => playScene(getScenePath(SceneFolderType.Final, mTitleScene));
    //[MenuItem("Jumper/Play Lobby Scene", priority = mPlayPriorityGroup)] public static void playLobbyScene() => playScene(getScenePath(SceneFolderType.Final, mLobbyScene));
    //[MenuItem("Jumper/Play Client Scene", priority = mPlayPriorityGroup)] public static void playClientScene() => playScene(getScenePath(SceneFolderType.Final, mClientScene));
    [MenuItem("Jumper/Play Server Scene", priority = mPlayPriorityGroup)] public static void playServerScene() => playScene(getScenePath(SceneFolderType.Final, mServerScene));
    //[MenuItem("Jumper/Play Final Stage Scene", priority = mPlayPriorityGroup)] public static void playFinalStageScene() => playScene(getScenePath(SceneFolderType.Final, mFinalStageScene));

    #region Legacy Scene Jumper

    // Legacy scene folder names
    private static readonly string mLegacyClientFolderName = "Client";
    private static readonly string mLegacyServerFolderName = "Server";
    private static readonly string mLegacyStageFolderName = "Stage";

    // Unity scene names
    private static readonly string mLegacyTitleSceneName = "TitleScene";
    private static readonly string mLegacyClientSceneName = "ClientScene";
    private static readonly string mLegacyServerSceneName = "ServerScene";

    private static readonly string mLegacyLobbyScene = "TestLobbyScene";

    private static readonly string mLegacyStageLobbyResourceSceneName = "Level 0/Lobby_Resource";
    private static readonly string mLegacyStageResourceSceneName_2 = "Level 1/Stage02_Resource";
    private static readonly string mLegacyStageT01_ResourceSceneName = "Level T0/StageT01_Resource";

    // Change unity scene
    [MenuItem("Jumper/Legacy Scene Change/LobbyScene", priority = mLegacySceneChangePriorityGroup)] public static void changeSceneToLegacyLobbyScene() => changeScene(getScenePath(SceneFolderType.None, mLegacyLobbyScene));
    [MenuItem("Jumper/Legacy Scene Change/ClientScene", priority = mLegacySceneChangePriorityGroup)] public static void changeSceneToLegacyClientScene() => changeScene(getScenePath(SceneFolderType.Client, mLegacyClientSceneName));
    [MenuItem("Jumper/Legacy Scene Change/ServerScene", priority = mLegacySceneChangePriorityGroup)] public static void changeSceneToLegacyServerScene() => changeScene(getScenePath(SceneFolderType.Server, mLegacyServerSceneName));

    // Game map scene priority group
    [MenuItem("Jumper/Legacy Scene Change/Lobby_Resource", priority = mLegacyGameMapScenePriorityGroup)] public static void changeSceneToLegacyLobbyResource() => changeScene(getScenePath(SceneFolderType.Stage, mLegacyStageLobbyResourceSceneName));
    [MenuItem("Jumper/Legacy Scene Change/Stage02_Resource", priority = mLegacyGameMapScenePriorityGroup)] public static void changeSceneToLegacyResource_2() => changeScene(getScenePath(SceneFolderType.Stage, mLegacyStageResourceSceneName_2));
    [MenuItem("Jumper/Legacy Scene Change/StageT01_Resource", priority = mLegacyGameMapScenePriorityGroup)] public static void changeSceneToLegacyT01_Resource() => changeScene(getScenePath(SceneFolderType.Stage, mLegacyStageT01_ResourceSceneName));

    // Instance play unity scene
    [MenuItem("Jumper/Legacy Play/Title Scene", priority = mLegacyPlayPriorityGroup)] public static void playLegacyTitleScene() => playScene(getScenePath(SceneFolderType.None, mLegacyTitleSceneName));
    [MenuItem("Jumper/Legacy Play/Client Scene", priority = mLegacyPlayPriorityGroup)] public static void playLegacyClientScene() => playScene(getScenePath(SceneFolderType.Client, mLegacyClientSceneName));
    [MenuItem("Jumper/Legacy Play/Server Scene", priority = mLegacyPlayPriorityGroup)] public static void playLegacyServerScene() => playScene(getScenePath(SceneFolderType.Server, mLegacyServerSceneName));

    #endregion

    #region Scene Management Functions
    public static void playScene(string scenePath)
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }

        changeScene(scenePath);
        EditorApplication.isPlaying = true;
    }

    public static void changeScene(string scenePath)
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }

        // If the scene has been modified, Editor ask to you want to save it.
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        EditorSceneManager.OpenScene(scenePath);
    }

    private static string getScenePath(SceneFolderType folderType, string sceneName)
    {
        return $"{mSceneReletivePath}{mSceneFolderTable[folderType]}/{sceneName}.unity";
    }

    private enum SceneFolderType
    {
        None,
        Client,
        Server,
        Stage,
        Final,
    }

    private static Dictionary<SceneFolderType, string> mSceneFolderTable = new Dictionary<SceneFolderType, string>()
    {
        { SceneFolderType.None, "" },
        { SceneFolderType.Client, mLegacyClientFolderName },
        { SceneFolderType.Server, mLegacyServerFolderName },
        { SceneFolderType.Stage, mLegacyStageFolderName },
        { SceneFolderType.Final, mFinalFolderName },
    };

    #endregion
}

#endif