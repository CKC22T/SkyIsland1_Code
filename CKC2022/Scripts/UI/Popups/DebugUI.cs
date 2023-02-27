using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CulterLib.UI.Popups
{
    public class DebugUI : PopupWindow
    {
        public static DebugUI Instance { get; private set; }
        #region Inspector
        [Title("Debug UI")]
        [TabGroup("Component"), SerializeField] private TextMeshProUGUI mText;
        [Title("Debug UI")]
        [TabGroup("Option"), SerializeField] private int mDeltaSaveCount = 20;
        #endregion
        #region Get,Set
        public bool IsOn { get => mText.gameObject.activeSelf; }
        #endregion
        #region Value
        private List<float> mDeltas = new List<float>();
        private float mFps = 0;
        private float mTcpPing = 0;
        private long mTcpSend = 0;
        private long mTcpRecv = 0;
        private float mUdpPing = 0;
        private long mUdpSend = 0;
        private long mUdpRecv = 0;
        #endregion

        #region Event
        //PopupWindow Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();

            Instance = this;
        }
        protected override void OnInitData()
        {
            base.OnInitData();

            mText.gameObject.SetActive(false);
        }

        //Unity Event
        private void Update()
        {
#if UNITY_EDITOR
            //`누르면 Active Toggle
            if (Input.GetKeyDown(KeyCode.BackQuote))
                mText.gameObject.SetActive(!mText.gameObject.activeSelf);

            if (!mText.gameObject.activeSelf)
                return;

            //FPS 업데이트
            if (mDeltaSaveCount <= mDeltas.Count)
                mDeltas.RemoveAt(0);
            mDeltas.Add(Time.deltaTime);
            mFps = 0;
            foreach (var v in mDeltas)
                mFps += v;
            mFps = 1 / (mFps / mDeltas.Count);

            //Text 업데이트
            mText.text = "";
            mText.text += $"FPS : {mFps:0.0}\n";
            mText.text += $"\n";
            mText.text += $"TCP Ping : {mTcpPing:0.0}ms\n";
            mText.text += $"TCP 송신량 : {mTcpSend}bps\n";
            mText.text += $"TCP 수신량 : {mTcpRecv}bps\n";
            mText.text += $"TCP 송수신량 : {mTcpSend + mTcpRecv}bps\n";
            mText.text += $"\n";
            mText.text += $"UDP Ping : {mUdpPing:0.0}ms\n";
            mText.text += $"UDP 송신량 : {mUdpSend}bps\n";
            mText.text += $"UDP 수신량 : {mUdpRecv}bps\n";
            mText.text += $"UDP 송수신량 : {mUdpSend + mUdpRecv}bps\n";
#endif
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// TCP Ping 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetTcpPing(float _ping)
        {
            mTcpPing = _ping;
            return this;
        }
        /// <summary>
        /// TCP 송신량 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetTcpSend(long _send)
        {
            mTcpSend = _send;
            return this;
        }
        /// <summary>
        /// TCP 수신량 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetTcpRecv(long _recv)
        {
            mTcpRecv = _recv;
            return this;
        }
        /// <summary>
        /// UDP Ping 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetUdpPing(float _ping)
        {
            mUdpPing = _ping;
            return this;
        }
        /// <summary>
        /// UDP 송신량 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetUdpSend(long _send)
        {
            mUdpSend = _send;
            return this;
        }
        /// <summary>
        /// UDP 수신량 설정
        /// </summary>
        /// <param name="_ping"></param>
        /// <returns></returns>
        public DebugUI SetUdpRecv(long _recv)
        {
            mUdpRecv = _recv;
            return this;
        }
        #endregion
    }
}