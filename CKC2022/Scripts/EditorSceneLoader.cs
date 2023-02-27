#if UNITY_EDITOR

using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class EditorSceneLoader : MonoBehaviour
{
	private string mStagePath = @"CKC2022/Scenes/Stage/";

	public string ResourcesSceneName;

	private Scene? mCurrentLoadedScene;

	private string mScenePath;

	public void Start()
	{
		mScenePath = Application.dataPath + '/' + mStagePath + ResourcesSceneName + ".unity";

		if (mCurrentLoadedScene.HasValue)
		{
			var operation = EditorSceneManager.UnloadSceneAsync(mCurrentLoadedScene.Value);
			operation.completed += sceneLoaded;
		}
		else
		{
            if (Application.isPlaying == false)
            {
			    mCurrentLoadedScene = EditorSceneManager.OpenScene(mScenePath, OpenSceneMode.Additive);
            }
		}
	}

	private void sceneLoaded(AsyncOperation operation)
	{
		mCurrentLoadedScene = EditorSceneManager.OpenScene(mScenePath, OpenSceneMode.Additive);
	}
}

#endif