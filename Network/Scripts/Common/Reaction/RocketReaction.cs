using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Network.Server;
using System;
using Network.Packet;

[Obsolete("Use New Type Reaction")]
public class RocketReaction : MonoBehaviour, IDetectorReactable
{
    public DetectorType ReactionDetectorType;
    public int Damage = 1;

    public void ReactionCallback(DetectorInfo detectorInfo, DetectedInfo detectedInfo)
    {
        DetectorInfo info = new DetectorInfo()
        {
            DamageInfo = new DamageInfo(Damage, FactionType.kNeutral), // We need to decide which damage is correct damage, 'this' or reduce something

            Direction = detectedInfo.normal, // Set normal of collide position.
            Origin = detectedInfo.hitPoint, // Set origin position by hit position;
            OwnerCollider = detectorInfo.OwnerCollider,
            OwnerEntityID = detectorInfo.OwnerEntityID
        };

        ServerMasterDetectorManager.Instance.CreateNewDetector(ReactionDetectorType, info);
    }

    //public Action<DetectorInfo, DetectedInfo> GetReactionCallback => (detectorInfo, detectedInfo) =>
    //{
    //    DetectorInfo info = new DetectorInfo()
    //    {
    //        Damage = this.Damage, // We need to decide which damage is correct damage, 'this' or reduce something

    //        Direction = detectedInfo.normal, // Set normal of collide position.
    //        Origin = detectedInfo.hitPoint, // Set origin position by hit position;
    //        OwnerCollider = detectorInfo.OwnerCollider,
    //        OwnerEntityID = detectorInfo.OwnerEntityID
    //    };

    //    ServerWorldManager.Instance.CreateNewDetector(ReactionDetectorType, info, null);
    //};
}
