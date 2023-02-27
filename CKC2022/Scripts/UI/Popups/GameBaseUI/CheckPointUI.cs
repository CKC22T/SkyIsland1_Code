using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckPointUI : MonoBehaviour
{
    [SerializeField] private int checkPointNumber = 0;

    [SerializeField] private Image checkPointPrograssBar;
    [SerializeField] private Image checkPointPing;

    [SerializeField] private Vector3 prograssStartPosition;
    [SerializeField] private Vector3 prograssEndPosition;
    [SerializeField] private GameObject[] checkPointLights;
    [SerializeField] private float[] prograssFillAmouts;
    [SerializeField] private CheckPointAreaNameUI[] areaNameUI;

    // Start is called before the first frame update
    void Start()
    {
        SetCheckPoint(0);
        SetPrograss(0.0f);
        SetPing();
        SetAreaName();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ClientSessionManager.Instance.TryGetCheckPointPrograss(out int currentCheckPoint, out float prograss))
        {
            if (checkPointNumber != currentCheckPoint)
            {
                SetCheckPoint(currentCheckPoint);
            }

            SetPrograss(prograss);
            SetPing();
        }
    }

    public void SetActiveAreaName(bool isActive)
    {
        foreach(var obj in areaNameUI)
        {
            obj.gameObject.SetActive(isActive);
        }
    }

    private void SetAreaName()
    {
        for (int i = 0; i < areaNameUI.Length; ++i)
        {
            areaNameUI[i].SetAreaName(GlobalAreaName.GetAreaName(i + 1));
        }
    }

    private void SetCheckPoint(int currentCheckPoint)
    {
        checkPointNumber = currentCheckPoint;

        foreach (var light in checkPointLights)
        {
            light.SetActive(false);
        }

        for (int i = 0; i < checkPointNumber; ++i)
        {
            checkPointLights[i].SetActive(true);
        }
    }

    private void SetPrograss(float prograss)
    {
        if (checkPointNumber == 0)
        {
            checkPointPrograssBar.fillAmount = prograssFillAmouts[0];
            return;
        }
        if (checkPointNumber == prograssFillAmouts.Length)
        {
            checkPointPrograssBar.fillAmount = 1.0f;
            return;
        }

        float prevPrograssFillAmout = prograssFillAmouts[checkPointNumber];
        float currentPrograssFillAmout = prograssFillAmouts[checkPointNumber + 1];

        checkPointPrograssBar.fillAmount = prevPrograssFillAmout + (currentPrograssFillAmout - prevPrograssFillAmout) * prograss;
    }

    private void SetPing()
    {
        float prograss = checkPointPrograssBar.fillAmount;
        checkPointPing.transform.localPosition = prograssStartPosition + (prograssEndPosition - prograssStartPosition) * prograss;
    }
}
