using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private float countNumber;

    public void Open(float count)
    {
        countNumber = count;
        setCountNumber();
    }

    // Update is called once per frame
    void Update()
    {
        countNumber -= Time.deltaTime;
        setCountNumber();
    }

    private void setCountNumber()
    {
        int count = Mathf.CeilToInt(countNumber);
        count = Mathf.Max(count, 0);

        countText.text = count.ToString();
    }
}
