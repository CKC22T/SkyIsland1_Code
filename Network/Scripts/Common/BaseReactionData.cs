using System;
using UnityEngine;

public interface IDetectorReactable
{
    //public Action<DetectorInfo, DetectedInfo> GetReactionCallback { get; }
    public void ReactionCallback(DetectorInfo detectorInfo, DetectedInfo detectedInfo);
}

