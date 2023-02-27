using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBoarRiseAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarSearchAction mSearchAction;
    [TabGroup("Option"), SerializeField] private float mRiseTime = 0.5f;
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mTime;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTime = 0;
        ParentMob.SetAnimatorTrigger("Rise");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTime += Time.fixedDeltaTime;
        if (mRiseTime <= mTime)
        {   //스캔시작
            return mSearchAction;
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        //피격ON
        ParentMob.Physics.TargetRigidbody.isKinematic = false;
        ParentMob.OwnerCollider.enabled = true;
    }
    #endregion
}
