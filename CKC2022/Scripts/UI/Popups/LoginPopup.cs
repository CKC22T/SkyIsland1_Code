using CulterLib.Presets;
using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class LoginPopup : PopupWindow
{
    public static LoginPopup Instance { get; private set; }
    #region Inspector
    [TabGroup("Component"), SerializeField] private TMP_InputField mNicknameInput;
    [TabGroup("Component"), SerializeField] private Control_Button mStartBtn;
    [TabGroup("Component"), SerializeField] private Control_Button mExitBtn;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    protected override void OnInitData()
    {
        base.OnInitData();
#if UNITY_EDITOR
        //UI씬인 경우 return
        if (UiTestManager.Instance)
            return;
#endif
        mStartBtn.OnBtnClickFunc += (_btn) =>
        {
            Login();
        };
        if (mExitBtn)
            mExitBtn.OnBtnClickFunc += (_btn) =>
            {
                Application.Quit();
            };
    }

    private void Update()
    {
        if (!NotifyPopup.Instance.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Login();
            }
        }
    }

    private void Login()
    {
        if (string.IsNullOrEmpty(mNicknameInput.text))
        {
            NotifyPopup.Instance.Open("Nickname Not Found", "이름을 입력해주세요.", new NotifyPopup.SBtnData("Common_Ok", null));
            return;
        }
        if (mNicknameInput.text.Length > 6)
        {
            NotifyPopup.Instance.Open("Nickname Long!!", "6글자 이하로 입력해주세요.", new NotifyPopup.SBtnData("Common_Ok", null));
            return;
        }

        ClientSessionManager.Instance.BindUsername(mNicknameInput.text);
        AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.TitleSceneName);

        return;
        BlockingPopup.Instance.Open();

        WebSockManager.Instance.Login(mNicknameInput.text, (ResponsePacket res) =>
        {
            BlockingPopup.Instance.Close();
            if (res == null)
            {
                Debug.Log($"WebSocket Close");
                NotifyPopup.Instance.Open("서버와 연결이 끊겼습니다.", "로컬플레이를 진행하시겠습니까?", new NotifyPopup.SBtnData("Common_Ok", () =>
                {
                    ClientSessionManager.Instance.BindUsername(mNicknameInput.text);
                    AsyncSceneLoader.Instance.AutoSceneChange("Title");
                }));
                return;
            }

            if (res.error != WebErrorCode.Success)
            {
                Debug.Log($"Login Error : {res.error.ToString()}");
                return;
            }

            if (!WebSockManager.Instance.ContainsParam(res, "nickname")) return;
            if (!WebSockManager.Instance.ContainsParam(res, "uuid")) return;

            string name = res.param["nickname"].ToString();
            int uuid = Convert.ToInt32(res.param["uuid"]);

            ClientSessionManager.Instance.BindUsername(name);

            LogSendManager.Instance.UserLog("Login", uuid, "");
            //LogSendManager.Instance.JoinTime(uuid, 32.0f);
            AsyncSceneLoader.Instance.AutoSceneChange("Title");

            //Close();
        });

        //BlockingPopup.Instance.Open();
        //GlobalManager.Instance.UserMgr.Login(new SigninReq(mNicknameInput.text, mNicknameInput.text), (code) =>
        //{
        //    BlockingPopup.Instance.Close();
        //    if (code == WebErrorCode.Success)
        //        Close();
        //    else
        //    {
        //        var mt = GlobalManager.Instance.DataMgr.GetTextTableData("Text_LoginPop_Fail").GetText();
        //        var st = GlobalManager.Instance.DataMgr.GetTextTableData($"Text_Error_{code}").GetText();
        //        NotifyPopup.Instance.Open(mt, st, new NotifyPopup.SBtnData("Common_Ok", null));
        //    }
        //});
        //Close();
    }
    #endregion
}
