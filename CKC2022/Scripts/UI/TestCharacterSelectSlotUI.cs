using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Network.Packet;

public class TestCharacterSelectSlotUI : MonoBehaviour
{
    [SerializeField] private TestLobbyGuiManager mLobbyManager;
    [SerializeField] private TextMeshProUGUI mUsername;

    [field : SerializeField] public EntityType CharacterType { get; private set; }

    public void SetupBySlotData(SessionSlot slotData = null)
    {
        if (slotData != null)
        {
            mUsername.text = slotData.Username.Value;
        }
        else
        {
            mUsername.text = GlobalTable.GetEntityName(CharacterType);
        }
    }

    public void OnClick_CharacterSelectionButton()
    {
        mLobbyManager.OnLobbyClick_SelectCharacter(CharacterType);
    }
}
