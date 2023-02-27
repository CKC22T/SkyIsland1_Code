using CulterLib.Presets;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

//GET room
[Serializable] public struct Address
{
    public string ip;
    public int port;
}
[Serializable] public struct RoomInfo
{
    public int id;
    public string name;
    public int maxUser;
    public int curUser;
    public Address addr;
}
[Serializable] public struct GetRoomReq
{
}
[Serializable] public class GetRoomRes : WebResponse
{
    //얘는 NewtonJson으로 파싱했음
}

//POST room
[Serializable] public struct CreateRoomReq
{
    public string name;

    public CreateRoomReq(string _name)
    {
        name = _name;
    }
}
[Serializable] public class CreateRoomRes : WebResponse
{
    public int id;
}

public class MatchingManager : MonoBehaviour
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private string m_URL = "223.222.228.50:3000";
    #endregion
    #region Get,Set
    public IReadOnlyList<RoomInfo> Rooms { get; } = new List<RoomInfo>();
    #endregion

    #region Event
    /// <summary>
    /// 현재 존재하는 방의 리스트를 가져옵니다.
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void UpdateRoomList(GetRoomReq _data, Action<WebErrorCodeTemp> _onEnd)
    {
        GlobalManager.Instance.WebMgr.Get($"http://{m_URL}/rooms", _data, (string _json, GetRoomRes _res) =>
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<string, RoomInfo>>(_json);
            (Rooms as List<RoomInfo>).Clear();
            foreach (var v in dic.Values)
                (Rooms as List<RoomInfo>).Add(v);

            _onEnd?.Invoke(_res.err);
        });
    }
    /// <summary>
    /// 방을 만듭니다.
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_onEnd"></param>
    public void CreateRoom(CreateRoomReq _data, Action<WebErrorCodeTemp, int> _onEnd)
    {
        GlobalManager.Instance.WebMgr.Post($"http://{m_URL}/rooms", _data, (string _json, CreateRoomRes _res) =>
        {
            _onEnd?.Invoke(_res.err, _res.id);
        });
    }
    #endregion
}
