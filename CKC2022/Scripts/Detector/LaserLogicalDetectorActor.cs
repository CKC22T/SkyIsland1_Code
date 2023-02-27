using CKC2022;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class LaserLogicalDetectorActor : MonoBehaviour
{
    [SerializeField]
    private LaserLogicalDetectorData data;

    [SerializeField]
    private LineRenderer beamLine;

    [SerializeField]
    private ParticleSystem beamStart;

    [SerializeField]
    private Transform beamEnd;
    private List<Renderer> beamEndRenderers;

    private bool isDetected = false;
    private Vector3 CachedPosition;
    private Vector3 LerpedEndPoint;
    private Vector3 HitNormal;

    private bool isInitialized = false;

    private AudioSource audio;


    private void Awake()
    {
        data.OnRotationUpdated += Data_OnRotationUpdated;
        data.OnDetected += Data_OnDetected;
        data.OnHitscanEnd += Data_OnHitscanEnd;
        data.OnHitscanStart += Data_OnHitscanStart;

        data.OnDetectStart += Data_OnDetectStart;
        data.OnDetectEnd += Data_OnDetectEnd;

        beamEndRenderers = beamEnd.GetComponentsInChildren<Renderer>().ToList();

        beamStart.Stop();
        beamLine.enabled = false;
        isInitialized = false;
    }

    private void Data_OnDetectStart()
    {
        var data = new SoundPlayData(transform.position);
        data.loop = true;

        audio = GameSoundManager.Play(SoundType.Weapon_Laser, data);
    }

    private void FixedUpdate()
    {
        LerpedEndPoint = Vector3.Lerp(LerpedEndPoint, CachedPosition, 0.48f);
    }

    private void OnEnable()
    {
        beamStart.Clear();
        isInitialized = false;
    }

    private void Data_OnHitscanStart()
    {
        isDetected = false;
    }

    private void Data_OnDetected(RaycastHit obj)
    {
        CachedPosition = obj.point;
        HitNormal = obj.normal;

        isDetected = true;
    }

    private void Data_OnRotationUpdated(Vector3 prev, Vector3 current)
    {
        CachedPosition = transform.position + current;

        if (isInitialized == false)
        {
            LerpedEndPoint = CachedPosition;

            beamStart.Play();
            beamLine.enabled = true;
        }
    }

    private void Data_OnHitscanEnd()
    {
        ShownBeamHit(isDetected);

        beamStart.transform.position = transform.position;
        beamEnd.transform.position = LerpedEndPoint;
        var dir = (LerpedEndPoint - transform.position).normalized;

        //testing
        if (data.TestSingleCollision(dir, out var hit))
        {
            beamEnd.transform.position = hit.point;
            HitNormal = hit.normal;
        }

        var lookAtPoint = isDetected ? beamEnd.position + HitNormal : beamStart.transform.position;
        beamEnd.transform.LookAt(lookAtPoint);
        beamStart.transform.LookAt(beamEnd);

        beamLine.SetPosition(0, beamStart.transform.position);
        beamLine.SetPosition(1, beamEnd.position);

        var distance = (beamEnd.position - beamStart.transform.position).magnitude;
        beamStart.transform.localScale = new Vector3(1, 1, distance * 0.1f);
    }

    private void ShownBeamHit(bool shown)
    {
        Debug.Log("shown : " + shown);
        foreach (var renderer in beamEndRenderers)
        {
            renderer.enabled = shown;
        }

        foreach (var renderer in beamEndRenderers)
        {
            renderer.renderingLayerMask = shown ? (uint)1 : (uint)0;
        }
    }

    private void Data_OnDetectEnd()
    {
        audio?.Stop();
    }

}