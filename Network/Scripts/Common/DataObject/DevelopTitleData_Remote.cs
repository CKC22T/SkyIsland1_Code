using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Network.Common
{
    public class DevelopTitleData_Remote : RemoteNetObject
    {
        [Sirenix.OdinInspector.ShowInInspector]
        public readonly NetStringData PlayerCount = new();

        [Sirenix.OdinInspector.ShowInInspector]
        public List<TitlePlayerData> players { get; private set; } = new List<TitlePlayerData>();

        public event Action OnInitialized;

        public override void InitializeData(in RemoteReplicationObject assignee)
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
    }
}