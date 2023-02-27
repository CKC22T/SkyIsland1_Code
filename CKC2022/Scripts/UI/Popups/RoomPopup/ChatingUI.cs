using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI chatingLog;
    [SerializeField] private TMP_InputField chatingField;
    [SerializeField] private Scrollbar scrollbar;

    public void Start()
    {
        //TODO: ChatingLog 받아오기
        chatingLog.text = "";
        ClientSessionManager.Instance.OnChatMessage += OnChatMessage;
    }

    private void OnChatMessage((string ChatUsername, string ChatMessage) message)
    {
        chatingLog.text += $"{message.ChatUsername} : {message.ChatMessage}\n";

        //Canvas.ForceUpdateCanvases();
        //if (chatingLog.rectTransform.sizeDelta.y > 0.0f)
        //{
        //    setChatingLogPositionY(chatingLog.rectTransform.sizeDelta.y);
        //}
    }

    private void setChatingLogPositionY(float posY)
    {
        if(chatingLog.rectTransform.sizeDelta.y < posY)
        {
            posY = chatingLog.rectTransform.sizeDelta.y;
        }
        var pos = chatingLog.rectTransform.anchoredPosition;
        pos.y = posY;
        chatingLog.rectTransform.anchoredPosition = pos;
    }

    public void OnScrollValueChange()
    {
        if(scrollbar.value <= 0.0f)
        {
            scrollbar.value = 0.0f;
            setChatingLogPositionY(chatingLog.rectTransform.sizeDelta.y);
        }
        if(scrollbar.value >= 1.0f)
        {
            scrollbar.value = 1.0f;
            setChatingLogPositionY(0);
        }
    }

    public void OnDestroy()
    {
        ClientSessionManager.Instance.OnChatMessage -= OnChatMessage;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            chatingField.Select();
        }
    }

    public void SendChatingMessage(string message)
    {
        chatingField.text = "";
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        //TODO: Send Chating Message
        ClientSessionManager.Instance.SendChatMessage(message);
        chatingField.ActivateInputField();
    }
}
