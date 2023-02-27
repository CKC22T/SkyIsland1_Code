using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Network.Packet;

public class MagicBoarSearchAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarHideAction mHideAction;
    [TabGroup("Option"), SerializeField, MinMaxSlider(1.0f, 5.0f, true)] private Vector2 mSearchTimeRange = new Vector2(2.0f, 4.0f);
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mSearchTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTime = 0;
        mTarget = null;
        mSearchTime = Random.Range(mSearchTimeRange.x, mSearchTimeRange.y);
        ParentManager.Detector.gameObject.SetActive(true);
        ParentMob.SetAnimatorTrigger("Search");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTime += Time.fixedDeltaTime;

        if (0 < ParentManager.Detector.Detected.Count)
        {   //가장 가까운 적으로 타겟 설정
            float ndist = float.MaxValue;
            MasterEntityData nentity = null;
            foreach (var v in ParentManager.Detector.Detected)
            {
                float dist = Vector3.Distance(ParentMob.transform.position, v.transform.position);
                if (ParentMob.FactionType.IsEnemy(v.FactionType) && dist < ndist)
                {
                    nentity = v;
                    ndist = dist;
                }
            }
            mTarget = nentity;
        }

        if (mTarget || mSearchTime <= mTime)
        {   //숨기
            return mHideAction.SetTarget(mTarget).SetType(MagicBoarHideAction.HideType.Idle);
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        ParentManager.Detector.gameObject.SetActive(false);
        StopAllCoroutines();
    }
    #endregion
}
