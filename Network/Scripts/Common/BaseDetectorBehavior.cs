using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IDetectorBehaviorModifiedable
{
    public void GetRigid(out Rigidbody rigid);
    
    public void GetDirection(out Vector3 direction);

    public void SetDirection(Vector3 direction);
}

public abstract class BaseDetectorBehavior : MonoBehaviour
{
    protected Rigidbody Rigid;
    protected Vector3 Direction;
    protected Vector3 DirectionNormalized;

    public void Initialize(Rigidbody rigid, DetectorInfo detectorDirection)
    {
        Rigid = rigid;
        Direction = detectorDirection.Direction;
        DirectionNormalized = Direction.normalized;
    }
}
