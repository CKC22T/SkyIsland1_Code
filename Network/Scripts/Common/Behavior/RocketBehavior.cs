using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBehavior : BaseDetectorBehavior, IDetectorBehaviorModifiedable
{
    public float Speed = 1;

    public void FixedUpdate()
    {
        transform.position += Speed * Time.fixedDeltaTime * DirectionNormalized;
    }

    public void GetDirection(out Vector3 direction)
        => direction = Direction;

    public void GetRigid(out Rigidbody rigid)
        => rigid = Rigid;

    public void SetDirection(Vector3 direction)
    {
        Direction = direction;
        DirectionNormalized = direction.normalized;

        transform.rotation = Quaternion.LookRotation(DirectionNormalized);
    }
}
