using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Types;
using CulterLib.UI.Controls;
using CulterLib.UI.Views;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using CulterLib.Presets;
using Utils;
using UnityEngine.UI;
using static CulterLib.UI.Popups.NotifyPopup;
using Network;

namespace CulterLib.UI.Popups
{
    public class TitlePopup : PopupWindow
    {
        public static TitlePopup Instance { get; private set; }
        #region Inspector
        [TabGroup("Component"), SerializeField] GraphicRaycaster mGraphicRaycaster;
        [TabGroup("Component"), SerializeField] Control_Button mOnlinePlayBtn;
        [TabGroup("Component"), SerializeField] Control_Button mLocalPlayBtn;
        [TabGroup("Component"), SerializeField] Control_Button mEmptyBtn;
        [TabGroup("Component"), SerializeField] Control_Button mEmptyInputFieldBtn;
        [TabGroup("Component"), SerializeField] Control_Button mMakeRoomBtn;
        [TabGroup("Component"), SerializeField] Control_Button mEnterRoomBtn;
        [TabGroup("Component"), SerializeField] Control_Button mMatchingBtn;
        [TabGroup("Component"), SerializeField] Control_Button mSinglePlayBtn;
        [TabGroup("Component"), SerializeField] Control_Button mLocalHostBtn;
        [TabGroup("Component"), SerializeField] Control_Button mIpPortBtn;
        [TabGroup("Component"), SerializeField] Control_Button mSettingBtn;
        [TabGroup("Component"), SerializeField] Control_Button mGoBackBtn;
        [TabGroup("Component"), SerializeField] Control_Button mCreditsBtn;
        [TabGroup("Component"), SerializeField] Control_Button mExitBtn;

        //IpPort
        [TabGroup("Component"), SerializeField] GameObject m_IpPortConObj;
        [TabGroup("Component"), SerializeField] TMP_InputField m_IpPortConInput;
        [TabGroup("Component"), SerializeField] Control_Button m_IpPortConBtn;

        //RoomNumber
        [TabGroup("Component"), SerializeField] GameObject m_RoomNumberConObj;
        [TabGroup("Component"), SerializeField] TMP_InputField m_RoomNumberConInput;
        [TabGroup("Component"), SerializeField] Control_Button m_RoomNumberConBtn;

        [TabGroup("Component"), SerializeField] private SystemLogUI systemLog;

        #endregion

        #region Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();

            Instance = this;
        }

        private bool TryGetIPEndPort(ResponsePacket res, out string ip, out int port)
        {
            ip = "";
            port = 0;

            if (!WebSockManager.Instance.ContainsParam(res, "ip")) return false;
            ip = res.param["ip"].ToString();

            if (!WebSockManager.Instance.ContainsParam(res, "port")) return false;
            port = Convert.ToInt32(res.param["port"]);

            return true;
        }

        private void JoinServer(string ip, int port)
        {
            if (!NetworkExtension.TryGetLocalIPAddressViaConnection(out var localAddress))
            {
                NotifyPopup.Instance.Open(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Error_UnityConnectError").GetText(), null, new NotifyPopup.SBtnData("Common_Ok", null));
                Debug.LogError("Fail TO Connect");
                return;
            }

            GlobalNetworkCache.SetLobbyInfo($"Room Number : {(port - ServerConfiguration.ServerInitialPortNumber)}");

            var ClientUdpPort = UnityEngine.Random.Range(51000, 60000);
            ClientNetworkManager.Instance.TryConnectToServer(ip, port, localAddress, ClientUdpPort);
            mGraphicRaycaster.enabled = false;
            Close();
        }

        private void OnlineButtonOn()
        {
            mOnlinePlayBtn.interactable = true;
        }

        private void OnlineButtonOff()
        {
            mOnlinePlayBtn.interactable = false;
        }

        private void SetOnlinePlayButton(bool isActive)
        {
            mMakeRoomBtn.gameObject.SetActive(isActive);
            mEnterRoomBtn.gameObject.SetActive(isActive);
            mMatchingBtn.gameObject.SetActive(isActive);
        }

        private void SetLocalPlayButton(bool isActive)
        {
            mSinglePlayBtn.gameObject.SetActive(isActive);
            mLocalHostBtn.gameObject.SetActive(isActive);
            mIpPortBtn.gameObject.SetActive(isActive);
        }

        private void SetTitleButton(bool isActive)
        {
            mOnlinePlayBtn.gameObject.SetActive(isActive);
            mLocalPlayBtn.gameObject.SetActive(isActive);
            mEmptyBtn.gameObject.SetActive(isActive);
            mCreditsBtn.gameObject.SetActive(isActive);
            mExitBtn.gameObject.SetActive(isActive);

            mGoBackBtn.gameObject.SetActive(!isActive);
        }

        private void WebSocketConnectCallback()
        {
            systemLog.Log("????????? ????????? ?????????????????????.");
            OnlineButtonOn();
        }

        private void WebSocketDisconnectCallback()
        {
            systemLog.Log("????????? ????????? ????????? ?????????????????????.");
            OnlineButtonOff();
        }

        private void OnDestroy()
        {
            WebSockManager.Instance.OnOpenCallback = null;
            WebSockManager.Instance.OnCloseCallback = null;
            ClientSessionManager.Instance.OnDisconnected -= onDisconnect;
        }

        protected override void OnStartClose()
        {
            base.OnStartClose();

            BlockingPopup.Instance.Close();
        }

        private void onDisconnect()
        {
            TitlePopup.Instance.Open();
            mGraphicRaycaster.enabled = true;
            if (GlobalNetworkCache.TryGetDisconnectedReason(out var reason))
            {
                NotifyPopup.Instance.Open("???????????? ????????? ???????????????.", reason, new NotifyPopup.SBtnData("Common_Cancel", null));
                GlobalNetworkCache.BindDisconnectReason("");
            }
            SingleServerController.Instance.KillProcess();
        }

        protected override void OnInitData()
        {
            base.OnInitData();
#if UNITY_EDITOR
            //UI?????? ?????? return
            if (UiTestManager.Instance)
                return;
#endif
            //???????????? ?????????
            ServerConfiguration.IsJoinInGame = false;

            m_IpPortConObj.SetActive(false);
            m_RoomNumberConObj.SetActive(false);

            if (!WebSockManager.Instance.IsConnected)
            {
                OnlineButtonOff();
                WebSockManager.Instance.OnOpenCallback = WebSocketConnectCallback;
                WebSockManager.Instance.OnCloseCallback = WebSocketDisconnectCallback;
                WebSockManager.Instance.Connect();
            }
            else
            {
                OnlineButtonOn();
            }

            ClientSessionManager.Instance.OnDisconnected += onDisconnect;

            SetOnlinePlayButton(false);
            SetLocalPlayButton(false);
            SetTitleButton(true);

            mOnlinePlayBtn.OnBtnClickFunc += (btn) =>
            {
                SetTitleButton(false);
                SetOnlinePlayButton(true);
            };
            mLocalPlayBtn.OnBtnClickFunc += (btn) =>
            {
                SetTitleButton(false);
                SetLocalPlayButton(true);
            };
            //????????? ?????????
            mMakeRoomBtn.OnBtnClickFunc += (btn) =>
            {   //TODO : ????????????
                BlockingPopup.Instance.Open();
                WebSockManager.Instance.CreateRoom((ResponsePacket res) =>
                {
                    if (res.error != WebErrorCode.Success)
                    {
                        NotifyPopup.Instance.Open("WebSocketError!", res.error.ToString(), new SBtnData("Common_Cancel", null));
                        BlockingPopup.Instance.Close();
                    }
                    else
                    {
                        if (TryGetIPEndPort(res, out var ip, out var port))
                        {
                            JoinServer(ip, port);
                        }
                    }
                });

            };
            mEnterRoomBtn.OnBtnClickFunc += (btn) =>
            {   //TODO : ???????????????
                m_RoomNumberConObj.SetActive(!m_RoomNumberConObj.activeSelf);
                mEmptyInputFieldBtn.gameObject.SetActive(!m_RoomNumberConObj.activeSelf);
            };
            mMatchingBtn.OnBtnClickFunc += (btn) =>
            {   //TODO : ????????????
                BlockingPopup.Instance.Open();
                WebSockManager.Instance.Match((ResponsePacket res) =>
                {
                    if (res.error != WebErrorCode.Success)
                    {
                        NotifyPopup.Instance.Open("WebSocketError!", res.error.ToString(), new SBtnData("Common_Cancel", null));
                        BlockingPopup.Instance.Close();
                    }
                    else
                    {
                        if (TryGetIPEndPort(res, out var ip, out var port))
                        {
                            JoinServer(ip, port);
                        }
                    }
                });
            };
            mSinglePlayBtn.OnBtnClickFunc += (btn) =>
            {
                BlockingPopup.Instance.Open();
                var ServerPort = UnityEngine.Random.Range(55000, 60000);
                //ServerPort = 50000;

                SingleServerController.Instance.StartProcess(Network.Server.ServerMode.SINGLE_MODE, ServerPort);
                StartCoroutine(Connect());
                IEnumerator Connect()
                {
                    yield return new WaitForSeconds(3.5f);
                    yield return null;
                    GlobalNetworkCache.SetLobbyInfo("Single Play");
                    var ClientUdpPort = UnityEngine.Random.Range(51000, 60000);
                    ClientNetworkManager.Instance.TryConnectToServer("127.0.0.1", ServerPort, "127.0.0.1", ClientUdpPort);
                    mGraphicRaycaster.enabled = false;
                    Close();

                    //int tryCount = 3;
                    //while(ClientNetworkManager.Instance.TryConnectToServer("127.0.0.1", ServerConfiguration.ServerInitialPortNumber, "127.0.0.1", ClientUdpPort) != NetworkErrorCode.SUCCESS)
                    //{
                    //    mGraphicRaycaster.enabled = false;
                    //    --tryCount;
                    //    if (tryCount < 0)
                    //    {
                    //        mGraphicRaycaster.enabled = true;
                    //        NotifyPopup.Instance.Open("????????? ?????????????????????..", "TimeOut", new NotifyPopup.SBtnData("Common_Cancel", null));
                    //        break;
                    //    }
                    //    yield return new WaitForSeconds(1.0f);
                    //}
                    //BlockingPopup.Instance.Close();
                }

            };
            mLocalHostBtn.OnBtnClickFunc += (btn) =>
            {
                BlockingPopup.Instance.Open();
                var ServerPort = UnityEngine.Random.Range(55000, 60000);
                //ServerPort = 50000;
                SingleServerController.Instance.StartProcess(Network.Server.ServerMode.USER_MODE, ServerPort);

                if (!NetworkExtension.TryGetLocalIPAddressViaConnection(out var localAddress))
                {
                    NotifyPopup.Instance.Open(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Error_UnityConnectError").GetText(), null, new NotifyPopup.SBtnData("Common_Ok", null));
                    Debug.LogError("Fail TO Connect");
                    return;
                }

                StartCoroutine(Connect(localAddress));

                IEnumerator Connect(string localAddress)
                {
                    yield return new WaitForSeconds(3.5f);
                    GlobalNetworkCache.SetLobbyInfo($"{localAddress}:{ServerPort}");
                    var ClientUdpPort = UnityEngine.Random.Range(51000, 60000);
                    ClientNetworkManager.Instance.TryConnectToServer(localAddress, ServerPort, localAddress, ClientUdpPort);
                    mGraphicRaycaster.enabled = false;
                    ClientNetworkManager.Instance.OnSessionConnected += (id) =>
                    {
                        //TitleManager.Instance.ConnectGame();
                    };
                    Close();
                }
            };
            mIpPortBtn.OnBtnClickFunc += (btn) =>
            {   //IpPort???????????? ???????????? IpPort ??????UI ??????
                m_IpPortConObj.SetActive(!m_IpPortConObj.activeSelf);
                mEmptyInputFieldBtn.gameObject.SetActive(!m_IpPortConObj.activeSelf);
            };
            mSettingBtn.OnBtnClickFunc += (btn) =>
            {   //????????????
                SettingPopup.Instance.Open();
            };
            mGoBackBtn.OnBtnClickFunc += (btn) =>
            {
                SetTitleButton(true);
                SetOnlinePlayButton(false);
                SetLocalPlayButton(false);
            };
            mCreditsBtn.OnBtnClickFunc += (btn) =>
            {   //?????????
                AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.VictoryCreditScene);
            };
            mExitBtn.OnBtnClickFunc += (btn) =>
            {   //?????? ??????
                Application.Quit();
            };
            m_IpPortConBtn.OnBtnClickFunc += (btn) =>
            {   //???????????? ?????? ?????? ?????? ????????? ?????? ??? ?????? ????????? ??????
                connectToIPPort();
            };
            m_RoomNumberConBtn.OnBtnClickFunc += (btn) =>
            {   //TODO: EnterRoom
                //??? ?????? ?????? ??? ????????????
                connectToRoomNumber();
            };
        }

        private void connectToRoomNumber()
        {
            if (int.TryParse(m_RoomNumberConInput.text, out int roomNumber))
            {
                BlockingPopup.Instance.Open();
                WebSockManager.Instance.LookUpRoom(roomNumber, (ResponsePacket res) =>
                {
                    BlockingPopup.Instance.Close();
                    if (res.error != WebErrorCode.Success)
                    {
                        NotifyPopup.Instance.Open("WebSocketError!", res.error.ToString(), new SBtnData("Common_Cancel", null));
                    }
                    else
                    {
                        if (TryGetIPEndPort(res, out var ip, out var port))
                        {
                            JoinServer(ip, port);
                        }
                    }
                });
            }
            else
            {
                NotifyPopup.Instance.Open("??? ????????? ???????????? ????????????..", "????????? ??????????????????.", new NotifyPopup.SBtnData("Common_Cancel", null));
            }
        }

        private void connectToIPPort()
        {
            var split = m_IpPortConInput.text.Split(':');
            if (split.Length == 2 && int.TryParse(split[1], out var port) && NetworkExtension.TryParseEndPoint(split[0], port, out var hostEP))
            {
                if (!NetworkExtension.TryGetLocalIPAddressViaConnection(out var localAddress))
                {
                    NotifyPopup.Instance.Open(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Error_UnityConnectError").GetText(), null, new NotifyPopup.SBtnData("Common_Ok", null));
                    Debug.LogError("Fail TO Connect");
                    return;
                }
                BlockingPopup.Instance.Open();

                GlobalNetworkCache.SetLobbyInfo($"IP:PORT : {m_IpPortConInput.text}");
                var ClientUdpPort = UnityEngine.Random.Range(51000, 60000);
                ClientNetworkManager.Instance.TryConnectToServer(split[0], port, localAddress, ClientUdpPort);
                mGraphicRaycaster.enabled = false;
                Close();
                //TitleManager.Instance.ConnectGame();
            }
            else
                NotifyPopup.Instance.Open(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Error_WrongIPPort").GetText(), null, new NotifyPopup.SBtnData("Common_Ok", null));
        }

        private void Update()
        {
            if (NotifyPopup.Instance && !NotifyPopup.Instance.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (m_RoomNumberConBtn.gameObject.activeInHierarchy)
                    {
                        connectToRoomNumber();
                    }

                    if (m_IpPortConBtn.gameObject.activeInHierarchy)
                    {
                        connectToIPPort();
                    }
                }
            }
        }
        #endregion
    }
}