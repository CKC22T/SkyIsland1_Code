using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingNone : LoadingOption
{
    protected override bool LoadingEnd()
    {
        return true;
    }

    protected override bool LoadingStart()
    {
        return true;
    }

    protected override void LoadingUpdate(AsyncOperation operation)
    {
    }
}
