using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class RangeHitscan : BaseDetectorData
{
    protected override void StartDestroyCoroutine()
    {
        DestroyCoroutine.StartSingleton(destroySelf());
    }

    private IEnumerator destroySelf()
    {
        yield return YieldInstructionCache.WaitForFixedUpdate;
        yield return YieldInstructionCache.WaitForFixedUpdate;

        ForceDestroy();
    }
}