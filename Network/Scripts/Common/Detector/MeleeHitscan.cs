using System.Collections;
using Utils;

public class MeleeHitscan : BaseDetectorData
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
