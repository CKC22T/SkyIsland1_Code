using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeWaveOnEvent : MonoBehaviour
{
    [SerializeField] private Transform startingPoint;
    [SerializeField] private List<BridgeEvent> bridges;
    [SerializeField] private int waveCount = 5;

    [SerializeField] private List<GameObject> invisibleWalls;

    #region TestCode
    public int waveNumber = 0;
    [Sirenix.OdinInspector.Button(Name = "Test Bridge Wave On")]
    public void TestBridgeWaveOn()
    {
        BridgeWaveOn(waveNumber);
    }
    #endregion

    [Sirenix.OdinInspector.Button(Name = "Set Sorting")]
    public void SetSorting()
    {
        bridges.Sort((bridgeA, bridgeB) =>
        {
            int i = 1;
            if (Vector3.Distance(bridgeA.destination.localPosition, startingPoint.position) >
            Vector3.Distance(bridgeB.destination.localPosition, startingPoint.position))
                i = -1;

            return i;
        });
    }

    public void BridgeWaveOn(int waveNumber)
    {
        foreach(var wall in invisibleWalls)
        {
            wall.SetActive(true);
        }
        for(int i =0; i <= waveNumber && i < invisibleWalls.Count; ++i)
        {
            invisibleWalls[i].SetActive(false);
        }

        ++waveNumber;
        int bridgeOnCount = Mathf.Min(bridges.Count * waveNumber / waveCount, bridges.Count);
        for (int i = 0; i < bridgeOnCount; ++i)
        {
            bridges[i].BridgeOn();
        }
    }
}
