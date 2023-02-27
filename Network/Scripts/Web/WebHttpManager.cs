using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Utils;

public enum WebErrorCode
{
    //Common
    Unknown = -1,                //상세정보가 없는 오류
    Success,                //성공
                            //UnityWebRequest Error
    UnityConnectError,              //연결 불가
    UnityProtocolError,             //프로토콜에러
    UnityDataError,                 //데이터처리 에러
}

public class WebHttpManager : MonoSingleton<WebHttpManager>
{
    [SerializeField] private int m_Timeout = 10;

    public void Get<TRequest>(string url, TRequest json, Action<WebErrorCode, string> onEnd)
    {
        StartCoroutine(GetCor(url, JsonConvert.SerializeObject(json), (err, res) =>
        {
            RequestCommon(err, res, onEnd);
        }));
    }

    public void Post<TRequest>(string url, TRequest json, Action<WebErrorCode, string> onEnd)
    {
        StartCoroutine(PostCor(url, JsonConvert.SerializeObject(json), (err, res) =>
        {
            RequestCommon(err, res, onEnd);
        }));
    }

    public void Put<TRequest>(string url, TRequest json, Action<WebErrorCode, string> onEnd)
    {
        StartCoroutine(PutCor(url, JsonConvert.SerializeObject(json), (err, res) =>
        {
            RequestCommon(err, res, onEnd);
        }));
    }

    public void Delete<TRequest>(string url, TRequest json, Action<WebErrorCode, string> onEnd)
    {
        StartCoroutine(DeleteCor(url, JsonConvert.SerializeObject(json), (err, res) =>
        {
            RequestCommon(err, res, onEnd);
        }));
    }

    private IEnumerator GetCor(string url, string json, Action<UnityWebRequest.Result, string> onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return RequestCommonCor(www, json, onEnd);
        }
    }

    private IEnumerator PostCor(string url, string json, Action<UnityWebRequest.Result, string> onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, json))
        {
            yield return RequestCommonCor(www, json, onEnd);
        }
    }

    private IEnumerator PutCor(string url, string json, Action<UnityWebRequest.Result, string> onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Put(url, json))
        {
            yield return RequestCommonCor(www, json, onEnd);
        }
    }

    private IEnumerator DeleteCor(string url, string json, Action<UnityWebRequest.Result, string> onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return RequestCommonCor(www, json, onEnd);
        }
    }

    private void RequestCommon(UnityWebRequest.Result r, string json, Action<WebErrorCode, string> onEnd)
    {
        var err = WebErrorCode.Success;
        switch (r)
        {
            case UnityWebRequest.Result.ConnectionError:
                err = WebErrorCode.UnityConnectError;
                break;
            case UnityWebRequest.Result.ProtocolError:
                err = WebErrorCode.UnityProtocolError;
                break;
            case UnityWebRequest.Result.DataProcessingError:
                err = WebErrorCode.UnityDataError;
                break;
        }
        onEnd?.Invoke(err, json);
    }

    private IEnumerator RequestCommonCor(UnityWebRequest _www, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        byte[] send = new System.Text.UTF8Encoding().GetBytes(_json);
        _www.uploadHandler = new UploadHandlerRaw(send);
        _www.downloadHandler = new DownloadHandlerBuffer();
        _www.timeout = m_Timeout;
        yield return _www.SendWebRequest();

        _onEnd?.Invoke(_www.result, _www.downloadHandler.text);
    }
}