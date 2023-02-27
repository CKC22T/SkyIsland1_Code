using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class LoadingOption : MonoBehaviour
{
    private AsyncOperation operation = null;
    private Coroutine loadCoroutine = null;

    public bool IsActive => operation != null;

    public void StartLoad(string changeSceneName, Action onLoaded, bool isAutoLoad)
    {
        loadCoroutine = StartCoroutine(Loading(changeSceneName, onLoaded, isAutoLoad));
    }

    public void StartLoad(string changeSceneName, LoadSceneParameters loadSceneParameters, Action onLoaded, bool isAutoLoad)
    {
        loadCoroutine =  StartCoroutine(Loading(changeSceneName, loadSceneParameters, onLoaded, isAutoLoad));
    }

    private IEnumerator Loading(string changeSceneName, Action onLoaded, bool isAutoLoad)
    {
        while (LoadingStart())
        {
            yield return null;
        }
        operation = SceneManager.LoadSceneAsync(changeSceneName);
        while (!operation.isDone)
        {
            LoadingUpdate(operation);
            yield return null;
        }
        onLoaded?.Invoke();
        if (isAutoLoad)
        {
            while (LoadingEnd())
            {
                yield return null;
            }
            operation = null;
        }
        else
        {
            while (true)
            {
                LoadingUpdate(operation);
                yield return null;
            }
        }
    }

    private IEnumerator Loading(string changeSceneName, LoadSceneParameters loadSceneParameters, Action onLoaded, bool isAutoLoad)
    {
        while (LoadingStart())
        {
            yield return null;
        }
        operation = SceneManager.LoadSceneAsync(changeSceneName, loadSceneParameters);
        while (!operation.isDone)
        {
            LoadingUpdate(operation);
            yield return null;
        }
        onLoaded?.Invoke();
        if (isAutoLoad)
        {
            while (LoadingEnd())
            {
                yield return null;
            }
            operation = null;
        }
        else
        {
            while (true)
            {
                LoadingUpdate(operation);
                yield return null;
            }
        }
    }

    public void EndLoad()
    {
        if (operation == null) return;

        StartCoroutine(LoadEnd());

        IEnumerator LoadEnd()
        {
            while (!operation.isDone)
            {
                yield return null;
            }
            while (LoadingEnd())
            {
                yield return null;
            }
            operation = null;
        }
    }

    public void StopLoad()
    {
        StopCoroutine(loadCoroutine);
    }

    protected abstract bool LoadingStart();
    protected abstract void LoadingUpdate(AsyncOperation operation);
    protected abstract bool LoadingEnd();
}
