using Network.Server;
using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;

public class TurretController : BaseLocationEventTrigger
{
    public static TurretState RemoteTurretState => ClientSessionManager.Instance.GameGlobalState.GameGlobalState.TurretState;
    public static TurretState MasterTurretState => ServerSessionManager.Instance.GameGlobalState.GameGlobalState.TurretState;

    [SerializeField] private int TurretID = 0;

    [SerializeField] private Renderer renderer1;
    [SerializeField] private Renderer renderer2;
    [SerializeField] private Renderer renderer3;
    private Material material1;
    private Material material2;
    private Material material3;
    [SerializeField] private Transform head;
    [SerializeField] private Transform rotate;
    [SerializeField] private Transform DetectorSocket;
    [SerializeField] private DetectorType TurretDetectorType;
    [SerializeField] private FactionType FactionType;
    [SerializeField] private int Damage;

    private Quaternion rotateEulerAnglesOrigin;
    [SerializeField] private float rotateSpeed;

    private Vector3 headOrigin;
    [SerializeField] private Vector3 headUpOffset;
    [SerializeField] private float headUpSpeed;

    [SerializeField] private int shotCount;
    [SerializeField] private Vector3 shotToEulerAngles;
    [SerializeField] private Vector3 shotFromEulerAngles;
    [SerializeField] private float shotDelay;

    private Coroutine turretCoroutine;
    public bool IsActive;

    [Sirenix.OdinInspector.Button(Name = "Test Run")]
    public void TestRun()
    {
        Run();
    }

    private void Start()
    {
        material1 = renderer1.material;
        material2 = renderer2.material;
        material3 = renderer3.material;

        rotateEulerAnglesOrigin = rotate.rotation;
        headOrigin = head.position;

    }

    public void Run()
    {
        if (turretCoroutine != null)
        {
            StopCoroutine(turretCoroutine);
        }
        turretCoroutine = StartCoroutine(runTurret());
    }

    private IEnumerator runTurret()
    {
        IsActive = true;
        SetDetectMaterial(false);

        Vector3 destinationPosition = headOrigin + headUpOffset;
        while (Vector3.Distance(head.position, destinationPosition) > 0.01f)
        {
            head.position = Vector3.Lerp(head.position, destinationPosition, Time.deltaTime * headUpSpeed);
            yield return null;
        }
        head.position = destinationPosition;

        yield return new WaitForSeconds(shotDelay);

        Quaternion destinationRotation = Quaternion.Euler(rotate.eulerAngles + shotToEulerAngles);
        while (Quaternion.Angle(rotate.rotation, destinationRotation) > 0.1f)
        {
            rotate.rotation = Quaternion.Slerp(rotate.rotation, destinationRotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        rotate.rotation = destinationRotation;

        Vector3 angle = shotFromEulerAngles - shotToEulerAngles;
        angle /= Mathf.Max(shotCount - 1, 1);
        for (int i = 0; i < shotCount; ++i)
        {
            //쏴
            if (ServerMasterDetectorManager.TryGetInstance(out var detectorManager))
            {
                if (DetectorSocket != null)
                {
                    DetectorInfo info = new DetectorInfo();

                    info.Origin = DetectorSocket.position;
                    info.Direction = DetectorSocket.forward;
                    info.RawViewVector = DetectorSocket.forward;
                    info.DamageInfo = new DamageInfo(Damage, FactionType);

                    detectorManager.CreateNewDetector(TurretDetectorType, info);
                }
            }

            //기다려
            yield return new WaitForSeconds(shotDelay);

            if (i >= shotCount - 1)
            {
                break;
            }

            //움직여
            Quaternion targetEulerAngles = Quaternion.Euler(rotate.eulerAngles + angle);
            while (Quaternion.Angle(head.rotation, targetEulerAngles) > 0.1f)
            {
                rotate.rotation = Quaternion.Slerp(rotate.rotation, targetEulerAngles, Time.deltaTime * rotateSpeed);
                yield return null;
            }
            rotate.rotation = targetEulerAngles;
        }

        End();
    }

    public void End()
    {
        if (turretCoroutine != null)
        {
            StopCoroutine(turretCoroutine);
        }
        StartCoroutine(endTurret());
    }

    private IEnumerator endTurret()
    {
        Vector3 destination = headOrigin;
        while (Vector3.Distance(head.position, destination) > 0.01f)
        {
            head.position = Vector3.Lerp(head.position, destination, Time.deltaTime * headUpSpeed);
            yield return null;
        }
        head.position = destination;

        SetDetectMaterial(true);

        yield return new WaitForSeconds(shotDelay);

        while (Quaternion.Angle(head.rotation, rotateEulerAnglesOrigin) > 0.1f)
        {
            rotate.rotation = Quaternion.Slerp(rotate.rotation, rotateEulerAnglesOrigin, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        rotate.rotation = rotateEulerAnglesOrigin;
        IsActive = false;
    }

    private void SetDetectMaterial(bool isDetected)
    {
        int value = isDetected? 1 : 0;
        material1.SetInt("_isDetected", value);
        material2.SetInt("_isDetected", value);
        material3.SetInt("_isDetected", value);
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;

        if (networkMode == NetworkMode.Master)
        {
            Run();
        }
    }

    private void FixedUpdate()
    {
        // 서버로서 터렛의 상태를 클라이언트에게 동기화
        if (ServerConfiguration.IS_SERVER)
        {
            MasterTurretState.TrySetTurretState(TurretID, IsActive);
        }

        // 클라이언트로서 터렛의 상태를 서버로 부터 전달받음
        if (ServerConfiguration.IS_CLIENT)
        {
            RemoteTurretState.TryGetTurretState(TurretID, out bool IsActive);
            SetDetectMaterial(!IsActive);
        }
    }
}
