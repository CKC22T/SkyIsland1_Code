using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPrograss : LoadingOption
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Image prograssImage;
    [SerializeField] private Image rollImage;

    [SerializeField] private float rotationPower;


    protected override bool LoadingStart()
    {
        tooltipText.text = "로딩 중...";
        canvasGroup.alpha = 1;
        return false;
    }

    protected override bool LoadingEnd()
    {
        canvasGroup.alpha = 0;
        return false;
    }

    protected override void LoadingUpdate(AsyncOperation operation)
    {
        //prograssImage.fillAmount = operation.progress;
        rollImage.transform.localRotation = Quaternion.Euler(rollImage.transform.localRotation.eulerAngles + -Vector3.forward * rotationPower * Time.deltaTime);
        prograssImage.transform.localRotation = Quaternion.Euler(prograssImage.transform.localRotation.eulerAngles + Vector3.forward * rotationPower * Time.deltaTime);
    }
}
