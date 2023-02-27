using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public override void InitializeData(in MasterReplicationObject assignee)

namespace Network.Common
{
    [Serializable]
    public class TitlePlayerData
    {
        public const int MaxPlayerCount = 4;

        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetStringData playerNameData = new();

        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetVector3Data InputData = new();

        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetVector3Data PositionData = new();

        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetVector3Data LookAtData = new();
    }

    public class DevelopTitleData_Master : MasterNetObject
    {
        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetStringData PlayerCount = new();

        [Sirenix.OdinInspector.ShowInInspector]
        public List<TitlePlayerData> players { get; private set; } = new List<TitlePlayerData>();

        public event Action OnInitialized;

        public override void InitializeData(in MasterReplicationObject assignee)
        {
            for (int i = 0; i < TitlePlayerData.MaxPlayerCount; i++)
            {
                var player = new TitlePlayerData();
                assignee.AssignDataAsReliable(player.playerNameData);
                assignee.AssignDataAsReliable(player.InputData);
                assignee.AssignDataAsReliable(player.PositionData);
                assignee.AssignDataAsReliable(player.LookAtData);

                players.Add(player);
            }

            assignee.AssignDataAsReliable(PlayerCount);

            //complete
            OnInitialized?.Invoke();
        }

        public void BindData(int sessionID, Request requestPacket)
        {
            var inputData = requestPacket.RequestTitleInput;

            var currentPlayer = players[inputData.SessionId];
            currentPlayer.InputData.Value = inputData.InputData.ToVector3();
            currentPlayer.PositionData.Value = inputData.PositionData.ToVector3();
            currentPlayer.LookAtData.Value = inputData.LookAtData.ToVector3();
        }
    }

}