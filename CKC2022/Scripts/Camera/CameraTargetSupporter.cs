using Network.Client;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Test;
using Utils;
using CKC2022.Input;

namespace CKC2022
{
    public class CameraTargetSupporter : LocalSingleton<CameraTargetSupporter>
    {
        [SerializeField]
        private CameraZoom zoom;

        [SerializeField]
        private Transform centerPoint;
        [SerializeField]
        private Vector3 centerOffset;

        [SerializeField]
        private Vector3 lookAtOffset;

        private Transform lookAtTarget;

        public readonly Notifier<ReplicatedEntityData> TrackingTarget = new Notifier<ReplicatedEntityData>();

        private CoroutineWrapper wrapper;


        protected override void Initialize()
        {
            base.Initialize();
            
            wrapper = new CoroutineWrapper(this);

            TrackingTarget.OnDataChanged += TrackingTarget_OnDataChanged;

            //ClientSessionManager.Instance.OnPlayerEntityChanged += OnPlayerAliveStateChanged;
            ResetCamera();
        }

        private void OnEnable()
        {
            ResetCamera();
        }

        private void TrackingTarget_OnDataChanged(ReplicatedEntityData target)
        {
            if (!target.TryGetComponent<PlaceHolder>(out var holder) || holder[PlaceHolder.PlaceType.ModelRoot] == null)
            {
                lookAtTarget = target.transform;
                return;
            }

            lookAtTarget = holder[PlaceHolder.PlaceType.ModelRoot];
        }


        //private void OnPlayerAliveStateChanged()
        //{
        //    wrapper.StartSingleton(Setup());

        //    IEnumerator Setup()
        //    {
        //        while(!SetupPlayer())
        //        {
        //            yield return null;
        //        }
        //    }
        //}

        private int mSpectatorIndex = 0;
        private bool mIsSpectatorMode = false;

        public void ResetCamera()
        {
            mSpectatorIndex = 0;
        }

        private void SwitchSpectatorIndexByInput()
        {
            if (mIsSpectatorMode)
            {
                if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1))
                {
                    mSpectatorIndex++;
                }
            }
        }

        private void TrySetupOrSetAsSpectator()
        {
            // Get client world
            if (!ClientWorldManager.TryGetInstance(out var worldManager))
            {
                return;
            }

            // Try setup mine
            if (worldManager.TryGetMyEntity(out var playerEntity))
            {
                setupCameraTrackPoint(playerEntity);
                mIsSpectatorMode = false;
                return;
            }

            //if (ClientSessionManager.Instance.TryGetMyPlayerEntityID(out int myPlayerEntityID))
            //{
            //    if (worldManager.TryGetMyEntity(out var playerEntity))
            //    {
            //        setupCameraTrackPoint(playerEntity);
            //        mIsSpectatorMode = false;
            //    }
            //}

            mIsSpectatorMode = true;

            // Try setup as spectator
            if (worldManager.TryGetPlayerEntities(out var playerEntities))
            {
                if (mSpectatorIndex >= playerEntities.Count)
                {
                    mSpectatorIndex = 0;
                }

                setupCameraTrackPoint(playerEntities[mSpectatorIndex]);
            }
        }

        private void setupCameraTrackPoint(ReplicatedEntityData target)
        {
            TrackingTarget.Value = target;
        }

        //public bool SetupPlayer()
        //{
        //    if (!enabled)
        //        return false;

        //    if (!ClientWorldManager.TryGetInstance(out var clientWorldManager))
        //        return false;

        //    if (clientWorldManager.TryGetMyEntity(out var myEntity) && myEntity.IsAlive.Value == true)
        //    {
        //        TrackingTarget.Value = myEntity;
        //        return true;
        //    }

        //    if (!clientWorldManager.TryGetPlayerEntities(out var entities))
        //        return false;

        //    for (int i = 0; i < entities.Count; ++i)
        //    {
        //        var next = entities.Next(TrackingTarget.Value);
        //        if (next != null && next.IsAlive.Value == true)
        //        {
        //            TrackingTarget.Value = next;
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private void Update()
        {
            SwitchSpectatorIndexByInput();
            TrySetupOrSetAsSpectator();

            //ViewNextPlayer();

            //if (Input)
        }

        //private void ViewNextPlayer()
        //{
        //    if (!ClientWorldManager.TryGetInstance(out var clientWorldManager))
        //        return;

        //    if (TrackingTarget.Value == null)
        //        return;

        //    if (TrackingTarget.Value.IsMine && TrackingTarget.Value.IsAlive.Value == true)
        //        return;

        //    //spector view mode
        //    if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1))
        //    {
        //        OnPlayerAliveStateChanged();
        //    }
        //}

        private void FixedUpdate()
        {
            if (lookAtTarget == null)
                return;

            centerPoint.position = lookAtTarget.position;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //if (ClientSessionManager.IsQuitting == false)
            //    ClientSessionManager.Instance.OnPlayerEntityChanged -= OnPlayerAliveStateChanged;
        }

    }
}