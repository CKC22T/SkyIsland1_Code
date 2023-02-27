using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretManageEvent : BaseLocationEventTrigger
{
    [SerializeField] private LocatorEnterEventDetector locator;
    [SerializeField] private Collider collider;

    [SerializeField] private List<TurretController> turrets;
    [SerializeField] private float restartDelay;
    public override void TriggeredEvent(BaseEntityData other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;

        if (networkMode == NetworkMode.Master)
        {
            StartCoroutine(turretRunning());
        }
    }

    private IEnumerator turretRunning()
    {
        foreach(var turret in turrets)
        {
            turret.Run();
            yield return new WaitUntil(() => !turret.IsActive);
        }

        yield return new WaitForSeconds(restartDelay);
        locator.IsActivated.Value = true;
        collider.isTrigger = true;
    }
}
