using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum WebErrorCodeTemp
{
    //Common
    Unknown = -1,                //상세정보가 없는 오류
    Success,                //성공
    //UnityWebRequest Error
    UnityConnectError,              //연결 불가
    UnityProtocolError,             //프로토콜에러
    UnityDataError,                 //데이터처리 에러
}
public abstract class WebResponse
{
    public WebErrorCodeTemp err = WebErrorCodeTemp.Success;
}

public class WebManager : MonoBehaviour
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private int m_Timeout = 10;
    #endregion

    #region Function
    //Public
    /// <summary>
    /// GET 요청하기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_url"></param>
    /// <param name="_json"></param>
    /// <param name="_onEnd"></param>
    public void Get<TRequest, TResponse>(string _url, TRequest _data, Action<string, TResponse> _onEnd) where TRequest : struct where TResponse : WebResponse, new()
    {
        StartCoroutine(GetCor(_url, JsonUtility.ToJson(_data), (_r, _json) =>
        {   //Response 도착하면 파싱해서 넘기기
            RequestCommon<TRequest, TResponse>(_r, _json, _onEnd);
        }));
    }
    /// <summary>
    /// POST 요청하기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_url"></param>
    /// <param name="_json"></param>
    /// <param name="_onEnd"></param>
    public void Post<TRequest, TResponse>(string _url, TRequest _data, Action<string, TResponse> _onEnd) where TRequest : struct where TResponse : WebResponse, new()
    {
        StartCoroutine(PostCor(_url, JsonUtility.ToJson(_data), (_r, _json) =>
        {   //Response 도착하면 파싱해서 넘기기
            RequestCommon<TRequest, TResponse>(_r, _json, _onEnd);
        }));
    }

    /// <summary>
    /// PUT 요청하기
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="_url"></param>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void Put<TRequest, TResponse>(string _url, TRequest _data, Action<string, TResponse> _onEnd) where TRequest : struct where TResponse : WebResponse, new()
    {
        StartCoroutine(PutCor(_url, JsonUtility.ToJson(_data), (_r, _json) =>
        {   //Response 도착하면 파싱해서 넘기기
            RequestCommon<TRequest, TResponse>(_r, _json, _onEnd);
        }));
    }

    /// <summary>
    /// DELETE 요청하기
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="_url"></param>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void Delete<TRequest, TResponse>(string _url, TRequest _data, Action<string, TResponse> _onEnd) where TRequest : struct where TResponse : WebResponse, new()
    {
        StartCoroutine(DeleteCor(_url, JsonUtility.ToJson(_data), (_r, _json) =>
        {   //Response 도착하면 파싱해서 넘기기
            RequestCommon<TRequest, TResponse>(_r, _json, _onEnd);
        }));
    }

    //Private
    /// <summary>
    /// GET 요청하는것 코루틴 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetCor(string _url, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_url))
        {
            yield return RequestCommonCor(www, _json, _onEnd);
        }
    }
    /// <summary>
    /// POST 요청하는것 코루틴 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator PostCor(string _url, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(_url, _json))
        {
            yield return RequestCommonCor(www, _json, _onEnd);
        }
    }

    /// <summary>
    /// PUT 요청하는것 코루틴 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator PutCor(string _url, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Put(_url, _json))
        {
            yield return RequestCommonCor(www, _json, _onEnd);
        }
    }

    /// <summary>
    /// DELETE 요청하는것 코루틴 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeleteCor(string _url, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        using (UnityWebRequest www = UnityWebRequest.Delete(_url))
        {
            yield return RequestCommonCor(www, _json, _onEnd);
        }
    }

    /// <summary>
    /// Request 공통부분 처리
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="_r"></param>
    /// <param name="_json"></param>
    private void RequestCommon<TRequest, TResponse>(UnityWebRequest.Result _r, string _json, Action<string, TResponse> _onEnd) where TRequest : struct where TResponse : WebResponse, new()
    {
        var res = (_r == UnityWebRequest.Result.Success) ? JsonUtility.FromJson<TResponse>(_json) : new TResponse();
        switch (_r)
        {
            case UnityWebRequest.Result.ConnectionError:
                res.err = WebErrorCodeTemp.UnityConnectError;
                break;
            case UnityWebRequest.Result.ProtocolError:
                res.err = WebErrorCodeTemp.UnityProtocolError;
                break;
            case UnityWebRequest.Result.DataProcessingError:
                res.err = WebErrorCodeTemp.UnityDataError;
                break;
        }
        _onEnd?.Invoke(_json, res);
    }
    /// <summary>
    /// Request 공통부분 코루틴 처리
    /// </summary>
    /// <param name="_www"></param>
    /// <param name="_json"></param>
    /// <param name="_onEnd"></param>
    /// <returns></returns>
    private IEnumerator RequestCommonCor(UnityWebRequest _www, string _json, Action<UnityWebRequest.Result, string> _onEnd)
    {
        byte[] send = new System.Text.UTF8Encoding().GetBytes(_json);
        _www.uploadHandler = new UploadHandlerRaw(send);
        _www.downloadHandler = new DownloadHandlerBuffer();
        _www.timeout = m_Timeout;
        yield return _www.SendWebRequest();

        _onEnd?.Invoke(_www.result, _www.downloadHandler.text);
    }
    #endregion
}
