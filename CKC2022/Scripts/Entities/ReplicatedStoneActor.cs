using Network.Client;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;


public class ReplicatedStoneActor : MonoBehaviour
{
    [SerializeField] private ReplicatedEntityData mEntityData;
    [SerializeField] private GameObject mDestroyDebrisPrefab;

    [SerializeField] private MeshFilter mMeshFilter;
    
    [SerializeField] private AnimationCurve ColorLerpCurve;
    [SerializeField] private float runtime;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    
    private int vertexCount;

    private Color[] Active;
    private Color[] Inactive;

    private CoroutineWrapper wrapper;
    private Mesh mesh;

    private void Awake()
    {
        wrapper = new CoroutineWrapper(this);
        //InitializeMesh();
    }

    private void InitializeMesh()
    {
        mesh = mMeshFilter.mesh;
        vertexCount = mesh.vertexCount;

        Active = Enumerable.Repeat(activeColor, vertexCount).ToArray();
        Inactive = Enumerable.Repeat(inactiveColor, vertexCount).ToArray();
    }

    public void OnEnable()
    {
        mEntityData.OnAction += OnAction;
        //mEntityData.OnHitAction += MEntityData_OnHitAction;
        mIsDestroyed = false;
    }

    //private void MEntityData_OnHitAction(ReplicatedDetectedInfo info)
    //{
    //    wrapper.StartSingleton(HitEffect());
    //}

    IEnumerator HitEffect()
    {
        float t = 0;
        while (t < runtime)
        {
            var value = Mathf.RoundToInt(ColorLerpCurve.Evaluate(t / runtime));
            mesh.SetColors(value == 1 ? Active : Inactive);
            t += Time.deltaTime;
            yield return null;
        }

        var last = Mathf.RoundToInt(ColorLerpCurve.Evaluate(1));
        mesh.SetColors(last == 1 ? Active : Inactive);
    }

    private bool mIsDestroyed = false;

    private void OnAction(EntityActionData actionData)
    {
        if (!actionData.HasAction)
        {
            return;
        }

        var actionType = actionData.Action;

        switch (actionType)
        {
            case EntityAction.kDestroy:
            case EntityAction.kDie:
                if (!mIsDestroyed)
                {
                    Vector3 position = transform.position;
                    Quaternion rotation = transform.rotation;

                    Instantiate(mDestroyDebrisPrefab, position, rotation);
                    PoolManager.ReleaseObject(mEntityData.gameObject);
                    mIsDestroyed = true;
                }
                break;
        }
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void Test()
    {
        wrapper.StartSingleton(HitEffect());
    }
#endif

}