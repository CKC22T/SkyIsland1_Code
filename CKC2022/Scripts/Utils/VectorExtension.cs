using UnityEngine;
using System.Collections;

namespace Utils
{
    public static class GameObjectExtension
    {
        public static GameObject SetAsRoot(this GameObject obj)
        {
            SetAsRoot(obj.transform);
            return obj;
        }

        public static Transform SetAsRoot(this Transform obj)
        {
            if (obj.parent != null)
                obj.SetParent(null, true);

            return obj;
        }
    }

    public static class PhysicsExtension
    {
        public static bool ComputePenetration(this Collider staticCollider, Collider dynamicCollider, out Vector3 dir, out float dis)
        {
            return Physics.ComputePenetration(
                dynamicCollider, dynamicCollider.transform.position, dynamicCollider.transform.rotation,
                staticCollider, staticCollider.transform.position, staticCollider.transform.rotation,
                out dir, out dis);
        }

        public static bool ComputePenetration(this Collider staticCollider, Collider dynamicCollider, Vector3 position, out Vector3 dir, out float dis)
        {
            return Physics.ComputePenetration(
                dynamicCollider, position, dynamicCollider.transform.rotation,
                staticCollider, staticCollider.transform.position, staticCollider.transform.rotation,
                out dir, out dis);
        }
    }



    public static class VectorExtension
    {
        public static float Remap(this float value, in float start1, in float stop1, in float start2, in float stop2) => start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));

        public static float Remap(this float value, in (float, float) input, in (float, float) output) => output.Item1 + (output.Item2 - output.Item1) * ((value - input.Item1) / (input.Item2 - input.Item1));

        public static Vector2 SetX(this in Vector2 vector, float x) => new Vector2(x, vector.y);
        public static Vector2 SetY(this in Vector2 vector, float y) => new Vector2(vector.x, y);

        public static Vector2 ToInvertY(this in Vector2 vector) => new Vector2(vector.x, -vector.y);
        public static Vector2 ToXZ(this in Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 ToVector2(this in Vector3 vector) => vector;
        public static Vector2 ToVector2(this in (float x, float y) tuple) => new Vector2(tuple.x, tuple.y);

        public static (float, float) ToTuple(this in Vector2 vector) => (vector.x, vector.y);
        public static (float, float, float) ToTuple(this in Vector3 vector) => (vector.x, vector.y, vector.z);


        public static Vector3 SetX(this in Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
        public static Vector3 SetY(this in Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
        public static Vector3 SetZ(this in Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        public static Vector3 ToVector3(this in Vector2 vector) => vector;
        public static Vector3 ToVector3(this in (float x, float y, float z) tuple) => new Vector3(tuple.x, tuple.y, tuple.z);
        public static Vector3 ToVector3(this in Vector2 vector, float z) => new Vector3(vector.x, vector.y, z);
        public static Vector3 ToVector3FromXZ(this in Vector2 xzVector) => new Vector3(xzVector.x, 0, xzVector.y);
        public static Vector3 ToVector3FromXZ(this in Vector2 xzVector, float y) => new Vector3(xzVector.x, y, xzVector.y);
        public static Vector3 AdaptY(this in Vector3 xzVector, in float y) => new Vector3(xzVector.x, y, xzVector.z);
        public static Vector3 MultiplyEachChannel(this in Vector3 lhs, in Vector3 rhs) => new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);

        public static Vector3 Round(this in Vector3 vector, in float scale) => new Vector3(
            Mathf.Round(vector.x / scale) * scale,
            Mathf.Round(vector.y / scale) * scale,
            Mathf.Round(vector.z / scale) * scale);

        public static Vector2 Decrease(this in Vector2 vector, in float amount) => new Vector2(
            Mathf.Sign(vector.x) * Mathf.Max(Mathf.Abs(vector.x) - amount, 0),
            Mathf.Sign(vector.y) * Mathf.Max(Mathf.Abs(vector.y) - amount, 0));
        public static Vector3 Decrease(this in Vector3 vector, in float amount) => new Vector3(
            Mathf.Sign(vector.x) * Mathf.Max(Mathf.Abs(vector.x) - amount, 0),
            Mathf.Sign(vector.y) * Mathf.Max(Mathf.Abs(vector.y) - amount, 0),
            Mathf.Sign(vector.z) * Mathf.Max(Mathf.Abs(vector.z) - amount, 0));

        //projection2D
        public static Vector2 ProjectionToXAxis(this Vector2 vector, Vector2 start, float xAxisValue)
        {
            return new Vector2(
                xAxisValue,
                (vector.y - start.y) * (xAxisValue - start.x) / (vector.x - start.x) + start.y);
        }
        public static Vector2 ProjectionToYAxis(this Vector2 vector, Vector2 start, float yAxisValue)
        {
            return new Vector2(
                (vector.x - start.x) * (yAxisValue - start.y) / (vector.y - start.y) + start.x,
                yAxisValue);
        }

        //projection3D
        public static Vector3 ProjectionToZAxis(this Vector3 vector, Vector3 start, float zAxisValue)
        {
            return new Vector3(
                (vector.x - start.x) * (zAxisValue - start.z) / (vector.z - start.z) + start.x,
                (vector.y - start.y) * (zAxisValue - start.z) / (vector.z - start.z) + start.y,
                zAxisValue);
        }
        public static Vector3 ProjectionToXAxis(this Vector3 vector, Vector3 start, float xAxisValue)
        {
            return new Vector3(
                xAxisValue,
                (vector.y - start.y) * (xAxisValue - start.x) / (vector.x - start.x) + start.y,
                (vector.z - start.z) * (xAxisValue - start.x) / (vector.x - start.x) + start.z);
        }
        public static Vector3 ProjectionToYAxis(this Vector3 vector, Vector3 start, float yAxisValue)
        {
            return new Vector3(
                (vector.x - start.x) * (yAxisValue - start.y) / (vector.y - start.y) + start.x,
                yAxisValue,
                (vector.z - start.z) * (yAxisValue - start.y) / (vector.y - start.y) + start.z);
        }

        public static Vector2 ToAbs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        public static Vector3 ToAbs(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        public static Vector3 IntersectionPoint(Vector3 origin, Vector3 target, Vector3 center, float radius)
        {
            var centerDir = center - origin;
            var forwardLength = Vector3.Project(centerDir, (target - origin).normalized);
            var orthogonal = Vector3.Distance(origin + forwardLength, center);

            if (orthogonal > radius)
                orthogonal = radius;

            var innerForward = Mathf.Sqrt((radius * radius) - (orthogonal * orthogonal));
            var dist = forwardLength.magnitude - innerForward;

            return origin + (target - origin).normalized * dist;
        }

        public static Vector3 GetRandomDirectionXZ()
        {
            return new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
        }

        public static Quaternion SlerpReverseUnClamped(Quaternion q1, Quaternion q2, float t)
        {
            float RawCosom =
                q1.x * q2.x +
                q1.y * q2.y +
                q1.z * q2.z +
                q1.w * q2.w;

            // 내적 판단을 거꾸로 한다.
            float Cosom = RawCosom >= 0 ? -RawCosom : RawCosom;
            float Scale0, Scale1;

            if (Cosom < 0.9999f)
            {
                Scale0 = 1.0f - t;
                Scale1 = t;
            }
            else
            {
                float Omega = Mathf.Acos(Cosom);
                float InvSin = 1.0f / Mathf.Sin(Omega);

                Scale0 = Mathf.Sin((1.0f - t) * Omega) * InvSin;
                Scale1 = Mathf.Sin(t * Omega) * InvSin;
            }

            // 적용도 거꾸로 한다.
            Scale1 = RawCosom >= 0 ? -Scale1 : Scale1;

            Quaternion Result;

            Result.x = Scale0 * q1.x + Scale1 * q2.x;
            Result.y = Scale0 * q1.y + Scale1 * q2.y;
            Result.z = Scale0 * q1.z + Scale1 * q2.z;
            Result.w = Scale0 * q1.w + Scale1 * q2.w;

            return Result;
        }

        ////angle
        //public static Vector2 RadianToVector2(this float radian)
        //{
        //    return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        //}

        //public static Vector2 DegreeToVector2(this float degree)
        //{
        //    return RadianToVector2(degree * Mathf.Deg2Rad);
        //}
        //public static Vector3 SnapTo(this Vector3 v3, float snapAngle)
        //{
        //    float angle = Vector3.Angle(v3, Vector3.up);

        //    if (angle < snapAngle / 2.0f)
        //        return Vector3.up * v3.magnitude;

        //    if (angle > 180.0f - snapAngle / 2.0f)
        //        return Vector3.down * v3.magnitude;

        //    float t = Mathf.Round(angle / snapAngle);
        //    float deltaAngle = (t * snapAngle) - angle;

        //    Vector3 axis = Vector3.Cross(Vector3.up, v3);
        //    Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
        //    return q * v3;
        //}

        //public static float GetDegreeFloatFromVector2(Vector2 vector2)
        //{
        //    return Mathf.Rad2Deg * Mathf.Atan2(vector2.y, vector2.x);
        //}

        //public static Vector2 SnapDirection(this Vector2 vector2)
        //{
        //    return DegreeToVector2(Mathf.Round(GetDegreeFloatFromVector2(vector2) / 90) * 90);
        //}
    }




}
