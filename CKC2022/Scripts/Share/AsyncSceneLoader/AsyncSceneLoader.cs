using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public enum LoadingOptionType
{
    None = 0,
    BlackFade = 1,
    Prograss = 2,
}

public class AsyncSceneLoader : MonoSingleton<AsyncSceneLoader>
{

    [SerializeField] private List<LoadingOption> LoadingOptions = new();
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private LoadingOption currentLoading = null;

    public bool IsLoading => currentLoading != null && currentLoading.IsActive;

    public void AutoSceneChange(string changeSceneName, LoadingOptionType loadingOptionType = LoadingOptionType.BlackFade, Action onLoaded = null)
    {
        if(targetSceneName.Equals(changeSceneName))
        {
            return;
        }
        targetSceneName = changeSceneName;
        SetLoadingOption(loadingOptionType);
        currentLoading.StartLoad(changeSceneName, onLoaded, true);
    }

    public void AutoSceneChange(string changeSceneName, LoadSceneParameters loadSceneParameters, LoadingOptionType loadingOptionType = LoadingOptionType.BlackFade, Action onLoaded = null)
    {
        if (targetSceneName.Equals(changeSceneName))
        {
            return;
        }
        targetSceneName = changeSceneName;
        SetLoadingOption(loadingOptionType);
        currentLoading.StartLoad(changeSceneName, loadSceneParameters, onLoaded, true);
    }

    public void SceneChange(string changeSceneName, LoadingOptionType loadingOptionType = LoadingOptionType.BlackFade, Action onLoaded = null)
    {
        //if (targetSceneName.Equals(changeSceneName))
        //{
        //    return;
        //}
        targetSceneName = changeSceneName;
        SetLoadingOption(loadingOptionType);
        currentLoading.StartLoad(changeSceneName, onLoaded, false);
    }

    public void SceneChange(string changeSceneName, LoadSceneParameters loadSceneParameters, LoadingOptionType loadingOptionType = LoadingOptionType.BlackFade, Action onLoaded = null)
    {
        if (targetSceneName.Equals(changeSceneName))
        {
            return;
        }
        targetSceneName = changeSceneName;
        SetLoadingOption(loadingOptionType);
        currentLoading.StartLoad(changeSceneName, loadSceneParameters, onLoaded, false);
    }

    private void SetLoadingOption(LoadingOptionType loadingOptionType)
    {
        if (currentLoading && currentLoading.IsActive)
        {
            currentLoading.StopLoad();
        }
        currentLoading = LoadingOptions[(int)loadingOptionType];
        // TODO: 예외처리 추가적으로 할 것
    }

    public void SceneChangeEnd()
    {
        currentLoading.EndLoad();
    }
}
