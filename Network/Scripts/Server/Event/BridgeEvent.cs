using Network.Client;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeEvent : BaseLocationEventTrigger
{
    [System.Serializable]
    public struct TransformInfo
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public void SetTransformInfo(Transform transform)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }
    }
    public TransformInfo destination;
    public TransformInfo startingPoint;
    public float moveSpeed = 3.0f;
    public bool IsDestroy = false;

    #region Offsets

    [SerializeField, LabelText("Pos MIN")] private Vector3 PositionOffsetMin;
    [SerializeField, LabelText("Pos MAX")] private Vector3 PositionOffsetMax;

    [SerializeField, LabelText("Rot MIN")] private Vector3 RotationOffsetMin;
    [SerializeField, LabelText("Rot MAX")] private Vector3 RotationOffsetMax;

    [SerializeField, LabelText("Scale MIN")] private Vector3 ScaleOffsetMin;
    [SerializeField, LabelText("Scale MAX")] private Vector3 ScaleOffsetMax;

    [SerializeField, LabelText("무너지는 정도")] private float offsetSize = 10.0f;

    #endregion


    public Rigidbody rigidbody => GetComponent<Rigidbody>();

    private Coroutine moveCoroutine = null;
    public event System.Action<bool> OnFloating;

    [Sirenix.OdinInspector.Button(Name = "Set Destination")]
    public void SetDestination()
    {
        destination.SetTransformInfo(transform);
    }

    [Sirenix.OdinInspector.Button(Name = "Set StartingPoint")]
    public void SetStartingPoint()
    {
        //Offset Random StartingPoint Setting
        startingPoint.SetTransformInfo(transform);
    }

    private void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }

    private void setOffsetMinMax(ref Vector3 min, ref Vector3 max)
    {
        if(min.x > max.x)
        {
            Swap(ref min.x, ref max.x);
        }
        if(min.y > max.y)
        {
            Swap(ref min.y, ref max.y);
        }
        if (min.z > max.z)
        {
            Swap(ref min.z, ref max.z);
        }
    }

    [Sirenix.OdinInspector.Button(Name = "Set Random StartingPoint")]
    public void SetRandomStartingPoint()
    {
        setOffsetMinMax(ref PositionOffsetMin, ref PositionOffsetMax);
        setOffsetMinMax(ref RotationOffsetMin, ref RotationOffsetMax);
        setOffsetMinMax(ref ScaleOffsetMin, ref ScaleOffsetMax);


        //Offset Random StartingPoint Setting
        startingPoint = destination;
        float x = Random.Range(PositionOffsetMin.x, PositionOffsetMax.x);
        float y = Random.Range(PositionOffsetMin.y, PositionOffsetMax.y);
        float z = Random.Range(PositionOffsetMin.z, PositionOffsetMax.z);
        startingPoint.localPosition += new Vector3(x, y, z);

        Vector3 angle = startingPoint.localRotation.eulerAngles;
        x = Random.Range(RotationOffsetMin.x, RotationOffsetMax.x);
        y = Random.Range(RotationOffsetMin.y, RotationOffsetMax.y);
        z = Random.Range(RotationOffsetMin.z, RotationOffsetMax.z);
        startingPoint.localRotation = Quaternion.Euler(angle + new Vector3(x, y, z));


        x = Random.Range(ScaleOffsetMin.x, ScaleOffsetMax.x);
        y = Random.Range(ScaleOffsetMin.y, ScaleOffsetMax.y);
        z = Random.Range(ScaleOffsetMin.z, ScaleOffsetMax.z);
        startingPoint.localScale = new Vector3(x, y, z);
    }

    [Sirenix.OdinInspector.Button(Name = "Set Position To Destination")]
    public void SetPositionToDestination()
    {
        transform.localPosition = destination.localPosition;
        transform.localRotation = destination.localRotation;
        transform.localScale = destination.localScale;
    }

    [Sirenix.OdinInspector.Button(Name = "Set Position To StartingPoint")]
    public void SetPositionToStartingPoint()
    {
        OnFloating?.Invoke(true);
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        transform.localPosition = startingPoint.localPosition;
        transform.localRotation = startingPoint.localRotation;
        transform.localScale = startingPoint.localScale;
    }

    [Sirenix.OdinInspector.Button(Name = "Test Bridge On")]
    public void TestBridgeOn()
    {
        OnFloating?.Invoke(false);
        BridgeOn();
    }

    [Sirenix.OdinInspector.Button(Name = "Test Bridge Off")]
    public void TestBridgeOff()
    {
        BridgeOff();
    }

    public void Start()
    {
        if(rigidbody.useGravity)
        {
            brokenBridge();
        }
        else
        {
            SetPositionToStartingPoint();
        }
    }

    private void brokenBridge()
    {
        float x = Random.Range(-offsetSize, offsetSize);
        float y = Random.Range(0, offsetSize);
        float z = Random.Range(-offsetSize, offsetSize);
        rigidbody.AddForce(new Vector3(x, y, z));
        rigidbody.AddTorque(new Vector3(x, y, z) * 0.1f);
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        OnFloating?.Invoke(false);
        BridgeOn();
    }

    public void BridgeOn()
    {
        //var rigidbody = GetComponent<Rigidbody>();
        //rigidbody.useGravity = false;
        //rigidbody.isKinematic = true;

        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(moveing());

        IEnumerator moveing()
        {
            //var objectAniControl = GetComponent<ObjectAniControl>();
            //objectAniControl.isActive = false;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            while ((destination.localPosition - transform.localPosition).magnitude > 0.01f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, destination.localPosition, Time.fixedDeltaTime * moveSpeed);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, destination.localRotation, Time.fixedDeltaTime * moveSpeed);
                transform.localScale = Vector3.Lerp(transform.localScale, destination.localScale, Time.fixedDeltaTime * moveSpeed);
                yield return new WaitForFixedUpdate();
            }

            transform.localPosition = destination.localPosition;
            transform.localRotation = destination.localRotation;
            transform.localScale = destination.localScale;
            //objectAniControl.SetOriginPos();
        }
    }

    public void BridgeOff()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(moveing());

        IEnumerator moveing()
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            brokenBridge();

            if (IsDestroy)
            {
                yield return new WaitForSeconds(5.0f);
                gameObject.SetActive(false);
            }
        }
    }
}
