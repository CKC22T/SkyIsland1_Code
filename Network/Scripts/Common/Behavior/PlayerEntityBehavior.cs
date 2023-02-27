using UnityEngine;
using Utils;

public static class PlayerEntityBehavior
{

    public class MovementSimulationInformation
    {
        public Vector2 moveDirection;

        //Never Vector2.zero;
        public Vector2 ChracterViewDirection;
        public float moveSpeed;
        public float deltaTime;
    }

    public class MovementResultInformation
    {
        public bool HasPosition;
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                HasPosition = true;
                position = value;
            }
        }

        public bool HasRotation;
        private Quaternion rotation;   
        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                HasRotation = true;
                rotation = value;
            }
        }
    }

    public static void ProcessMoving(Transform transform, CharacterPhysics physics, in MovementSimulationInformation info)
    {
        if (!CalculateMoving(transform, info, out var result))
            return;

        if (result.HasPosition)
            physics.Move(info.moveDirection);
            //transform.position = result.Position;

        if (result.HasRotation)
            transform.rotation = result.Rotation;
    }

    public static bool CalculateMoving(Transform transform, in MovementSimulationInformation info, out MovementResultInformation result)
    {
        if (info.ChracterViewDirection.magnitude <= Vector2.kEpsilon)
        {
            //Debug.LogError("info.inputViewDirection is Zero");
            result = null;
            return false;
        }

        Vector3 moveTo = info.deltaTime * info.moveSpeed * info.moveDirection.ToVector3FromXZ();
        Vector2 viewDirection = info.ChracterViewDirection;

        result = new MovementResultInformation();
        if (moveTo.magnitude > Vector3.kEpsilon)
            result.Position = transform.position + moveTo;

        if (viewDirection != Vector2.zero)
            result.Rotation = Quaternion.LookRotation(viewDirection.ToVector3FromXZ());

        return result.HasPosition || result.HasRotation;
    }

}