using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiTestManager : MonoBehaviour
{
    public static UiTestManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
