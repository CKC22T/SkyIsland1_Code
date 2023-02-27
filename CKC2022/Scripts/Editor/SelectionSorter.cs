using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Utils;


#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;

using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;


public class SelectionSorter : OdinEditorWindow
{
    [MenuItem("Window/SelectionSorter", false, 2)]
    static void Init()
    {
        SelectionSorter window = CreateInstance<SelectionSorter>();
        window.Show();
    }

    private static readonly Func<Vector3, float> HorizontalSelector = (vec) => vec.x;
    private static readonly Func<Vector3, float> VerticalSelector = (vec) => vec.y;

    [Sirenix.OdinInspector.Button("Align Left")] public void SetLeft() => SetAlign(HorizontalSelector, 0f);

    [Sirenix.OdinInspector.Button("Align Center")] public void SetCenter() => SetAlign(HorizontalSelector, 0.5f);

    [Sirenix.OdinInspector.Button("Align Right")] public void SetRight() => SetAlign(HorizontalSelector, 1f);



    [Sirenix.OdinInspector.PropertySpace(10)]
    [Sirenix.OdinInspector.Button("Align Top")] public void SetTop() => SetAlign(VerticalSelector, 1f);

    [Sirenix.OdinInspector.Button("Align Middle")] public void SetMiddle() => SetAlign(VerticalSelector, 0.5f);

    [Sirenix.OdinInspector.Button("Align Bottom")] public void SetBottom() => SetAlign(VerticalSelector, 0);


    private void SetAlign(in Func<Vector3, float> alignSelector, in float weight)
    {
        RecodeObjects();
        GetSelectedGameObject(alignSelector, out var sortedSelection);

        var alignedPosition = alignSelector(GetTotalPosition(sortedSelection, weight));

        foreach (var target in sortedSelection)
        {
            var viewPosition = ConvertToView(target.transform.position);
            var axis = GetAxisFormSelector(alignSelector);

            //position += aliengPosition - defaultPosition for axis
            //same as viewPosition = axis * alignedPosition + (viewPosition - viewPosition.MultiplyEachChannel(axis));
            viewPosition += axis * alignedPosition - viewPosition.MultiplyEachChannel(axis);

            target.transform.position = ConvertToWorld(viewPosition);
        }


        //LocalFunctions
        Vector3 GetTotalPosition(in List<GameObject> selections, in float factor)
            => Vector3.Lerp(
                ConvertToView(selections.First().transform.position),
                ConvertToView(selections.Last().transform.position),
                factor);
    }


    [Sirenix.OdinInspector.PropertySpace(10)]
    [Sirenix.OdinInspector.Button("Distribute Horizontally")] public void DistributeHorizontally() => Distribute(HorizontalSelector);

    [Sirenix.OdinInspector.Button("Distribute Vertically")] public void DistributeVertically() => Distribute(VerticalSelector);


    private void Distribute(in Func<Vector3, float> alignSelector)
    {
        if (Selection.objects.Length < 3)
            return;

        RecodeObjects();
        GetSelectedGameObject(alignSelector, out var sortedSelection);

        var count = sortedSelection.Count - 1;
        var start = ConvertToView(sortedSelection.First().transform.position);
        var end = ConvertToView(sortedSelection.Last().transform.position);

        var totalInterval = alignSelector(end - start);
        for (int i = 1; i < sortedSelection.Count - 1; ++i)
        {
            var viewPosition = ConvertToView(sortedSelection[i].transform.position);
            var axis = GetAxisFormSelector(alignSelector);

            //same as viewPosition = axis * alignedPosition + (viewPosition - viewPosition.MultiplyEachChannel(axis));
            //if Axis is Y, viewPosition = y + (default)xz
            viewPosition = (alignSelector(start) + (totalInterval / count) * i) * axis + (viewPosition - viewPosition.MultiplyEachChannel(axis));

            sortedSelection[i].transform.position = ConvertToWorld(viewPosition);
        }
    }

    //
    private void GetSelectedGameObject(Func<Vector3,float> alignSelector, out List<GameObject> sortedSelection)
    {
        var SceneCamera = SceneView.lastActiveSceneView.camera.transform;
        sortedSelection = Selection.objects.Select(SelectToGameObject).Where(obj => obj != null).ToList();
        sortedSelection.Sort(new Comparison<GameObject>((left, right)
            => alignSelector(ConvertToView(left.transform.position)).CompareTo(alignSelector(ConvertToView(right.transform.position)))
       ));


        //Local Functions
        GameObject SelectToGameObject(UnityEngine.Object obj)
        {
            return obj switch
            {
                GameObject go => go,
                Transform transform => transform.gameObject,
                Component component => component.gameObject,
                _ => null
            };
        }
    }

    private void RecodeObjects()
    {
        Undo.RecordObjects(Selection.objects.Select(SelectToTransform).Where(obj => obj != null).ToArray(), MethodBase.GetCurrentMethod().Name);


        //Local Functions
        Transform SelectToTransform(UnityEngine.Object obj)
        {
            return obj switch
            {
                GameObject go => go.transform,
                Transform transform => transform,
                Component component => component.transform,
                _ => null
            };
        }
    }

    private Vector3 GetAxisFormSelector(in Func<Vector3, float> alignSelector)
    {
        var axis = alignSelector(new Vector3(1, 2, 4));
        return axis switch
        {
            1 => Vector3.right,
            2 => Vector3.up,
            4 => Vector3.forward,
            _ => Vector3.zero,
        };
    }

    private Vector3 ConvertToView(in Vector3 worldPos)
        => SceneView.lastActiveSceneView.camera.transform.InverseTransformPoint(worldPos);

    private Vector3 ConvertToWorld(in Vector3 sceneCameraViewPosition)
        => SceneView.lastActiveSceneView.camera.transform.TransformPoint(sceneCameraViewPosition);

}

#else
public class SelectionSorter : MonoBehaviour
{
}
#endif
