using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderTagUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetName(string name)
    {
        nameText.text = name;
    }
}
