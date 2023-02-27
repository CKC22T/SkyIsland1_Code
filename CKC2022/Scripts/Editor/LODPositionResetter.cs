using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

public class LODPositionResetter : OdinEditorWindow
{
    public Transform Root;

    [Sirenix.OdinInspector.Button("Setup")]
    public void Setup()
    {
        var targets = Root.GetComponentsInChildren<LODGroup>();
        foreach (var target in targets)
        {
            SetTransform(target);
        }
    }


    [MenuItem("Window/LODPositionResetter")]
    static void Init()
    {
        LODPositionResetter window = CreateInstance<LODPositionResetter>();
        window.Show();
    }


    public void SetTransform(in LODGroup group)
    {
        Vector3 displacement = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        foreach (Transform child in group.transform)
        {
            if (!child.TryGetComponent<MeshFilter>(out var mesh))
                continue;

            displacement += child.localPosition;
            rot *= Quaternion.Inverse(child.localRotation);

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        group.transform.position = group.transform.TransformPoint(displacement);
        group.transform.rotation *= rot;

        Debug.Log($"{group.transform.name}'s transform modified. position is {displacement}, rotation is {rot}");
    }
}

#else
public class LODPositionResetter : MonoBehaviour
{
}
#endif
