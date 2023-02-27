using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotActionBase : CharacterAction
{
    #region Type
    /// <summary>
    /// 다음으로 진행할 액션 정보
    /// </summary>
    private struct NextActInfo
    {
        public float actWeight;
        public float randWeight;

        public NextActInfo(float _actWeight)
        {
            actWeight = _actWeight;
            randWeight = 0;
        }
        public NextActInfo SetRandWeight(float _randWeight)
        {
            var info = this;
            info.randWeight = _randWeight;
            return info;
        }
    }
    #endregion

    #region Inspector
    [Title("BotActionBase")]
    [TabGroup("Option"), SerializeField, MinMaxSlider(0, 5, true)] private Vector2 mTimeRange = new Vector2(0.5f, 2.0f);
    [TabGroup("Option"), SerializeField] private float m_BaseWeight = 1.0f;
    [TabGroup("Option"), SerializeField] private float mCooltimeAddWeight;
    [TabGroup("Option"), SerializeField] private float mDangerHp;
    [TabGroup("Option"), SerializeField] private float mDangerAddWeight;
    [TabGroup("Option"), SerializeField] private bool mNeedPlayer;
    #endregion
    #region Const
    private static float vWeightDist = 0.1f;    //가장 높은 Weight - vWeightDist 가중치의 액션까지 확률에 따라 실행됨 (원래 테이블에 넣어야하는변수임)
    #endregion
    #region Value
    private float mTime;
    private float mTimer;
    #endregion

    #region Event
    //CharacterAction Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTimer = 0;
        mTime = Random.Range(mTimeRange.x, mTimeRange.y);
    }
    protected override CharacterAction OnUpdate()
    {
        mTimer += Time.deltaTime;
        if (mTimer < mTime)
            return this;
        else
            return GetNextAction();
    }
    protected override CharacterAction OnFixedUpdate()
    {
        if (mTimer < mTime)
            return this;
        else
            return GetNextAction();
    }

    //BotAiActionBase Event
    protected virtual float OnWeight()
    {
        if (mNeedPlayer)
        {
            bool isPlayer = false;
            foreach (var v in ServerPlayerCharacterManager.Instance.PlayerEntities)
                if (0 <= v.BindedClientID.Value)
                    isPlayer = true;
            if (!isPlayer)
                return 0;
        }

        float weight = m_BaseWeight;

        if (ParentHuman.HasWeapon && !ParentHuman.CanFire)
            weight += mCooltimeAddWeight;

        if (ParentHuman.Hp.Value / (float)ParentHuman.MaxHp <= mDangerHp)
            weight += mDangerAddWeight;

        return weight;
    }
    #endregion
    #region Function
    //Protected
    /// <summary>
    /// 타이머 끝내기
    /// </summary>
    protected void EndTimer()
    {
        mTimer = mTime;
    }

    //Private
    /// <summary>
    /// 다음으로 진행할 Action을 가져옵니다.
    /// </summary>
    /// <returns></returns>
    CharacterAction GetNextAction()
    {
        //모든 액션의 가중치 구하기
        var actWeight = new Dictionary<BotActionBase, NextActInfo>();
        foreach (var v in ParentManager.ChildActions)
            if (v is BotActionBase a)
                actWeight.Add(a, new NextActInfo(a.OnWeight()));

        //가장 높은 가중치의 액션 가져오기
        var bigAct = this;
        var bigWeight = actWeight[this].actWeight;
        foreach (var v in actWeight)
            if (bigWeight < v.Value.actWeight)
            {
                bigAct = v.Key;
                bigWeight = v.Value.actWeight;
            }

        //랜덤뽑기시의 가중치 설정
        float totalRand = 0;
        foreach (var v in ParentManager.ChildActions)
            if (v is BotActionBase a)
            {
                var value = actWeight[a];
                value.randWeight = Mathf.InverseLerp(bigWeight - vWeightDist, bigWeight, value.actWeight);
                totalRand += value.randWeight;
                actWeight[a] = value;
            }

        //랜덤뽑기
        float rand = Random.Range(0, totalRand);
        foreach(var v in actWeight)
        {
            rand -= v.Value.randWeight;
            if (rand <= 0)
                return v.Key;
        }
        return this;
    }
    /// <summary>
    /// 기존에 계산된 weight값보다 해당 액션의 값이 더 클 경우 값을 가져옵니다.
    /// </summary>
    /// <param name="_oriweight"></param>
    /// <param name="_weight"></param>
    /// <returns></returns>
    private bool TryGetBiggerWeight(float _oriweight, out float _weight)
    {
        _weight = OnWeight();
        return _oriweight < _weight;
    }
    #endregion
}
