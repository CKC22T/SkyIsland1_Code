using System.Collections;
using System.Collections.Generic;
using CKC2022;
using CulterLib.Types;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace CulterLib.Presets
{
    public class TitleManager : LocalSingleton<TitleManager>
    {
        #region Inspector
        [TabGroup("Component"), SerializeField] private Camera mMainCam;
        [TabGroup("Component"), SerializeField] private Transform mTitlePos;
        [TabGroup("Component"), SerializeField] private Transform mLobbyPos;
        [TabGroup("Option"), SerializeField] private string m_TitleSceneName = "Title";
        [TabGroup("Option"), SerializeField] private string m_LobbySceneName = "Lobby";
        [TabGroup("Option"), SerializeField] private AnimationCurve mCamCurve;
        [TabGroup("Option"), SerializeField] private float mCamSpeed = 1.0f;
        #endregion

        #region Event
        //Unity Event
        private void Start()
        {
            PoolManager.ClearPool();

            //기본 초기화
            UIManager.Instance.Init();

            if (GlobalNetworkCache.TryGetDisconnectedReason(out var reason))
            {
                NotifyPopup.Instance.Open("서버와의 연결이 끊겼습니다.", reason, new NotifyPopup.SBtnData("Common_Cancel", null));
                GlobalNetworkCache.BindDisconnectReason("");
            }

            //로고 및 타이틀 열기
            //if (GlobalManager.Instance.UserMgr.UUID < 0)
            //    LoginPopup.Instance.Open();
        }
        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && UIManager.Instance.PopMgr.OpenedPopup[UIManager.Instance.PopMgr.OpenedPopup.Count - 1] == RoomPopup.Instance)
            {
                ExitPopup.Instance.Open();
            }
        }
        #endregion
        #region Function
        //Public
        public void ConnectGame()
        {
            if (mMainCam)
                StartCoroutine(ConnectCor());
            else
                RoomPopup.Instance.Open();
        }
        public void ToLobby()
        {
            LoadingPopup.Instance.Open();
            GlobalManager.Instance.LoadMgr.Load_All(loadProgress, loadEnd);

            void loadProgress(float _pro)
            {   //로딩 퍼센트 표시
                LoadingPopup.Instance.SetProgress(_pro);
            }
            void loadEnd(bool _isSuc)
            {   //로딩 후 게임씬으로, 실패시 타이틀씬 리로드
                if (_isSuc)
                    GlobalManager.Instance.SceneChangeMgr.SceneChange(m_LobbySceneName);
                else
                {
                    Destroy(GlobalManager.Instance.gameObject);
                    SceneManager.LoadScene(m_TitleSceneName);
                }
            }
        }

        //Private
        private IEnumerator ConnectCor()
        {
            float timer = 0;
            while(timer < 1.0f)
            {
                timer += Time.deltaTime * mCamSpeed;

                float lerp = mCamCurve.Evaluate(timer);
                mMainCam.transform.position = Vector3.Lerp(mTitlePos.position, mLobbyPos.position, lerp);
                mMainCam.transform.rotation = Quaternion.Slerp(mTitlePos.rotation, mLobbyPos.rotation, lerp);
                yield return null;
            }

            RoomPopup.Instance.Open();
        }
        #endregion
    }
}