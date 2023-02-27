using CulterLib.Presets;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

//signup
[Serializable] public struct SignupReq
{
    public string id;
    public string pw;
    public string nick;

    public SignupReq(string _id, string _pw, string _nick)
    {
        id = _id;
        pw = _pw;
        nick = _nick;
    }
}
[Serializable] public class SignupRes : WebResponse
{
}

//login
[Serializable] public struct SigninReq
{
    public string id;
    public string pw;

    public SigninReq(string _id, string _pw)
    {
        id = _id;
        pw = _pw;
    }
}
[Serializable] public class SigninRes : WebResponse
{
    public int uuid;
}

//userinfo
[Serializable] public struct UserinfoReq
{
    public int uuid;

    public UserinfoReq(int _uuid)
    {
        uuid = _uuid;
    }
}
[Serializable] public class UserinfoRes : WebResponse
{
    public string nick;
}

public class UserManager : MonoBehaviour
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private string m_URL = "223.222.228.50:3000";
    #endregion
    #region Get,Set
    /// <summary>
    /// 플레이어 ID
    /// </summary>
    public int UUID { get; private set; } = -1;
    public string Nickname { get; private set; }
    #endregion

    #region Function
    /// <summary>
    /// 회원가입합니다.
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void Signup(SignupReq _data, Action<WebErrorCodeTemp> _onEnd)
    {
        GlobalManager.Instance.WebMgr.Post($"http://{m_URL}/signup", _data, (string _json, SignupRes _res) =>
        {
            _onEnd?.Invoke(_res.err);
        });
    }
    /// <summary>
    /// 로그인합니다.
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void Login(SigninReq _data, Action<WebErrorCodeTemp> _onEnd)
    {
        GlobalManager.Instance.WebMgr.Post($"http://{m_URL}/login", _data, (string _json, SigninRes _res) =>
        {
            UUID = (_res.err == WebErrorCodeTemp.Success) ? _res.uuid : -1;
            _onEnd?.Invoke(_res.err);
        });
    }
    /// <summary>
    /// 유저정보가져옵니다.
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void GetUserinfo(UserinfoReq _data, Action<WebErrorCodeTemp> _onEnd)
    {
        GlobalManager.Instance.WebMgr.Post($"http://{m_URL}/getuserinfo", _data, (string _json, UserinfoRes _res) =>
        {
            Nickname = _res.nick;
            _onEnd?.Invoke(_res.err);
        });
    }
    #endregion
}
