using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Utils;

public class CreditManager : MonoBehaviour
{
    public PlayableDirector timeline;

    public CoroutineWrapper Wrapper;

    private void Awake()
    {
        GlobalNetworkCache.SetOnVictoryCredit(false);

        if (Wrapper == null)
        {
            Wrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }

        Wrapper.StartSingleton(timelineRoutine());
    }

    private IEnumerator timelineRoutine()
    {
        yield return new WaitUntil(() => timeline.state == PlayState.Paused);

        Debug.Log("Pause");

        AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.TitleSceneName);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            timeline.Pause();
        }
    }
}
