using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlineUserCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI onlineUserCountText;
    private void FixedUpdate()
    {
        onlineUserCountText.text = $"Online User : {GlobalNetworkCache.GetOnlineUserCount()}";
    }
}
