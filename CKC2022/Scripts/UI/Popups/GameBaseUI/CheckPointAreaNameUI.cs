using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckPointAreaNameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI areaNameText;

    public void SetAreaName(string areaName)
    {
        areaNameText.text = areaName;
        Canvas.ForceUpdateCanvases();
        var rect = transform as RectTransform;
        rect.sizeDelta = new Vector2(areaNameText.rectTransform.sizeDelta.x + 20, rect.sizeDelta.y);
    }
}
