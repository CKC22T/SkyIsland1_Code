using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Utils;
using System;

namespace Test
{
    public static class MinMaxExtension
    {
        public static float EvaluateAsRange(this Vector2 vector, in float t)
        {
            return Mathf.LerpUnclamped(vector.x, vector.y, t);
        }
    }

    public class CameraZoom : LocalSingleton<CameraZoom>
    {
        [SerializeField]
        private AutoFocus autoFocus;
        [SerializeField]
        private CameraFollow cameraFollow;

        [Header("Height")]
        [MinMaxSlider(-10, 10, true)]
        [SerializeField]
        private Vector2 Height;
        [SerializeField]
        private AnimationCurve HeightCurve;

        [Header("View")]
        [MinMaxSlider(0.01f, 20, true)]
        [SerializeField]
        private Vector2 ViewWidth;
        [SerializeField]
        private AnimationCurve ViewCurve;

        [Header("FOV")]
        [MinMaxSlider(10, 60, true)]
        [SerializeField]
        private Vector2 InvFov;
        [SerializeField]
        private AnimationCurve FovCurve;


        private float TargetRatio;
        private float currentRatio;

        [SerializeField]
        private float wheelSensitivity;

        private readonly Notifier<int> ScrollDelta = new Notifier<int>();
        public readonly Notifier<int> ViewLevel = new Notifier<int>();


        protected override void Initialize()
        {
            base.Initialize();

            currentRatio = 1f;
            TargetRatio = 1f;

            ScrollDelta.OnDataChanged += ScrollDelta_OnDataChanged;
            ViewLevel.OnDataChanged += ViewLevel_OnDataChanged;
        }


        private void ScrollDelta_OnDataChanged(int delta)
        {
            var value = ViewLevel.Value - delta;
            ViewLevel.Value = Mathf.Clamp(value, -1, 1);
        }

        private void ViewLevel_OnDataChanged(int level)
        {
            TargetRatio = VectorExtension.Remap(level, (-1, 1), (0, 1));
        }

        private void Update()
        {
            ScrollDelta.Value = Math.Sign(Input.mouseScrollDelta.y);
        }


        private void FixedUpdate()
        {
            currentRatio = Mathf.Lerp(currentRatio, TargetRatio, 0.16f);
            UpdateValue(currentRatio);
        }

        private void UpdateValue(in float currentRatio)
        {
            var targetHeight = Height.EvaluateAsRange(HeightCurve.Evaluate(currentRatio));
            var targetWidth = ViewWidth.EvaluateAsRange(ViewCurve.Evaluate(currentRatio));
            var targetFov = InvFov.EvaluateAsRange(1 - FovCurve.Evaluate(currentRatio));

            autoFocus.fovRange = InvFov;
            autoFocus.ViewWidth = targetWidth;
            autoFocus.ViewHeight = targetHeight;
            autoFocus.TargetFOV = targetFov;

            cameraFollow.UpdateLookAtOffset(targetHeight);

            cameraFollow.overrideFactor = (1 - currentRatio) * 0.5f;
        }


    }
}