using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomUserListUI : MonoBehaviour
{
    public List<TextMeshProUGUI> UserNameList;

    // Start is called before the first frame update
    void Start()
    {
        UpdateUserList();
    }

    private void FixedUpdate()
    {
        UpdateUserList();
    }

    private void UpdateUserList()
    {
        var userList = ClientSessionManager.Instance.UserSessionData.SessionSlots.GetConnectedSlots();

        foreach(var item in UserNameList)
        {
            item.gameObject.SetActive(false);
        }

        int i = 0;
        foreach(var user in userList)
        {
            UserNameList[i].gameObject.SetActive(true);
            UserNameList[i].text = user.Username.Value;
            ++i;
            if(i >= UserNameList.Count)
            {
                break;
            }
        }

        for (; i < UserNameList.Count; ++i)
        {

            UserNameList[i].text = "";
        }
    }
}
