using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CulterLib.Presets
{
    public class LobbyManager : LocalSingleton<TitleManager>
    {
        #region Event
        //Unity Event
        private void Start()
        {
            //기본 초기화
            UIManager.Instance.Init();

            CKC2022.GameSoundManager.PlayBGM(CKC2022.SoundType.Lobby_BackGround);
        }
        #endregion
    }
}