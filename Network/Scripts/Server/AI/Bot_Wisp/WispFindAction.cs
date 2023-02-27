using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network.Packet;

public class WispFindAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mFindEndAction;
    [TabGroup("Option"), SerializeField] private float mFindAniTime = 1.84f;
    #endregion
    #region Value
    private float mTimer;
    #endregion

    #region Event
    protected override CharacterAction OnFixedUpdate()
    {
        if (mTimer <= 0)
        {
            foreach (var v in ParentManager.Detector.Detected)
                if (v && ParentHuman.FactionType.IsEnemy(v.FactionType))
                {
                    mTimer = mFindAniTime;
                    ParentHuman.ActionTrigger("findStart");
                    break;
                }
        }
        else
        {
            mTimer -= Time.fixedDeltaTime;
            if (mTimer <= 0)
            {
                ParentHuman.ActionTrigger("findEnd");
                return mFindEndAction;
            }
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        //TODO : wisp_find 애니메이션 끝 (기존 애니메이션 재생)
    }
    #endregion
}
