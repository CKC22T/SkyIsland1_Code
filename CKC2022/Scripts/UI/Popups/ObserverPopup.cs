using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using TMPro;
using CulterLib.UI.Controls;

public class ObserverPopup : PopupWindow
{
    public static ObserverPopup Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI mTargetName;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();

        Instance = this;
    }
    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        if (CKC2022.CameraTargetSupporter.TryGetInstance(out var cameraTargetSupporter))
            cameraTargetSupporter.TrackingTarget.OnChanged += OnTrackingTargetChanged;
    }
    protected override void OnEndClose()
    {
        base.OnEndClose();

        if (CKC2022.CameraTargetSupporter.TryGetInstance(out var cameraTargetSupporter))
            cameraTargetSupporter.TrackingTarget.OnChanged -= OnTrackingTargetChanged;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (CKC2022.CameraTargetSupporter.TryGetInstance(out var cameraTargetSupporter))
            cameraTargetSupporter.TrackingTarget.OnChanged -= OnTrackingTargetChanged;
    }

    //Data Event
    private void OnTrackingTargetChanged()
    {
        if (!CKC2022.CameraTargetSupporter.TryGetInstance(out var cameraTargetSupporter))
            return;

        var targetData = cameraTargetSupporter.TrackingTarget.Value;
        var n = ClientSessionManager.Instance.UserSessionData.GetUsernameByCharacterType(targetData.EntityType);

        mTargetName.text = string.IsNullOrEmpty(n) ? targetData.EntityType.GetEntityName() : n;
    }
    #endregion
}
