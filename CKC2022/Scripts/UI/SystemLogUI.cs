using CKC2022;
using CulterLib.UI.Popups;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemLogUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    private Coroutine logCoroutine;

    private void Start()
    {
        canvasGroup.alpha = 0.0f;
        ClientSessionManager.Instance.OnSystemMessageCallback += Log;
    }

    private void OnDestroy()
    {
        if (ClientSessionManager.IsQuitting == false)
            ClientSessionManager.Instance.OnSystemMessageCallback -= Log;
    }

    public void Log(string logString)
    {
        GameSoundManager.Play(SoundType.UI_InGame, new SoundPlayData(transform.position));
        text.text = logString;
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
        }
        logCoroutine = StartCoroutine(showLog(1.2f, 0.2f));

        IEnumerator showLog(float showTime, float alphaTime)
        {
            float timer = 0.0f;
            canvasGroup.alpha = 0.0f;

            while (canvasGroup.alpha < 1)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = timer / alphaTime;
                yield return null;
            }
            canvasGroup.alpha = 1;

            timer = 0.0f;
            while (timer < showTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0.0f;
            while (canvasGroup.alpha > 0)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = 1 - timer / alphaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;
        }
    }

    public void Log(string logString, float showTime, float alphaTime)
    {
        text.text = logString;
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
        }
        logCoroutine = StartCoroutine(showLog(showTime, alphaTime));

        IEnumerator showLog(float showTime, float alphaTime)
        {
            float timer = 0.0f;
            canvasGroup.alpha = 0.0f;

            while (canvasGroup.alpha < 1)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = timer / alphaTime;
                yield return null;
            }
            canvasGroup.alpha = 1;

            timer = 0.0f;
            while (timer < showTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0.0f;
            while (canvasGroup.alpha > 0)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = 1 - timer / alphaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;
        }
    }
}
