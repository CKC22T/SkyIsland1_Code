using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMoveAction : CharacterAction
{
    #region Type
    [System.Serializable] private struct Pattern
    {
        public SpiritPatternAction action;
        public float weight;
    }
    #endregion

    #region Inspector
    [TabGroup("Component"), SerializeField] private Pattern[] mPattern1;
    [TabGroup("Component"), SerializeField] private Pattern[] mPattern2;
    [TabGroup("Component"), SerializeField] private float mPatternDelay = 1.0f; //n초내로 패턴 사용 못하면 다른 패턴으로 교체
    [TabGroup("Option"), SerializeField] private float mSpeed1 = 3.5f;
    [TabGroup("Option"), SerializeField] private float mSpeed2 = 3.675f;
    #endregion
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    private SpiritPatternAction mCurPattern;
    private float mTimer;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        if (ParentSpirit.MaxHp / 2 <= ParentSpirit.Hp.Value)
            ParentSpirit.Agent.speed = mSpeed1;
        else
            ParentSpirit.Agent.speed = mSpeed2;

        mCurPattern = GetAction(GetPattern()); ;
        mTimer = mPatternDelay;
        ParentSpirit.SetAnimatorTrigger("Walk");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        //페이즈2로 넘어갈 조건이면 넘어감
        if (ParentSpirit.TryGetChangePhase(out var next))
            return next;

        float dist = Vector3.Distance(ParentSpirit.transform.position, mTarget.transform.position);
        ParentSpirit.Agent.SetDestination(mTarget.transform.position);

        //충분히 가까워지면 설정된 패턴 실행
        if (Vector3.Distance(ParentSpirit.transform.position, mTarget.transform.position) <= mCurPattern.Distance)
            return mCurPattern.SetTarget(mTarget);
        else
        {
            mTimer -= Time.deltaTime;
            if (mTimer <= 0)
            {
                mCurPattern = GetAction(GetPattern());
                mTimer = mPatternDelay;
            }

            return this;
        }
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        ParentSpirit.Agent.SetDestination(ParentSpirit.transform.position);
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 타겟을 설정합니다.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public SpiritMoveAction SetTarget(MasterEntityData _target)
    {
        mTarget = _target;
        return this;
    }

    //Private
    private Pattern[] GetPattern()
    {
        if (ParentSpirit.MaxHp / 2 <= ParentSpirit.Hp.Value)
            return mPattern1;
        else
            return mPattern2;
    }
    /// <summary>
    /// 사용할 패턴을 가져옵니다.
    /// </summary>
    /// <param name="_pattern"></param>
    /// <returns></returns>
    private SpiritPatternAction GetAction(Pattern[] _pattern)
    {
        float total = 0;
        foreach (var v in _pattern)
            total += v.weight;

        float random = Random.Range(0, total);
        foreach(var v in _pattern)
        {
            random -= v.weight;
            if (random <= 0)
                return v.action;
        }
        return _pattern[0].action;
    }
    #endregion
}
