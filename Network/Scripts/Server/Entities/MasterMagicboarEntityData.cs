using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MagicBoarAttackAction;

public class MasterMagicboarEntityData : MasterMobEntityData
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarAttackAction mAttack;
    [TabGroup("Component"), SerializeField] private MagicBoarDamagedAction mDamaged;
    #endregion
    #region Value
    private int mLastedHp;
    #endregion

    #region Event
    private void Start()
    {
        Hp.OnChanged += () =>
        {
            if ((mActionManager.CurrentActions[0] != mAttack || mAttack.State == AttackState.Done) && Hp.Value < mLastedHp)
            {
                if (mActionManager.CurrentActions[0] == mDamaged)
                    mDamaged.ResetAni();
                else
                    mActionManager.SetAction(mDamaged);
            }

            mLastedHp = Hp.Value;
        };
    }
    #endregion
}
