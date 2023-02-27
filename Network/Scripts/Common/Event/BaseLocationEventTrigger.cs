using UnityEngine;

public abstract class BaseLocationEventTrigger : MonoBehaviour
{
    public abstract void TriggeredEvent(BaseEntityData other);
}
