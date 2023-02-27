using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemberTagUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject readyText;
    [SerializeField] private GameObject unReadyText;
    public bool IsReady { get; private set; } = false;

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void Ready()
    {
        readyText.SetActive(true);
        unReadyText.SetActive(false);
        IsReady = true;
    }

    public void UnReady()
    {
        readyText.SetActive(false);
        unReadyText.SetActive(true);
        IsReady = false;
    }
}
