using CKC2022;
using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Popups;
using Network.Client;
using Network.Packet;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using TMPro;

public class HpBar : Mono_UI
{
    #region Inspector
    [Title("HpBar")]
    [TabGroup("Component"), SerializeField] private Image PreGauge;
    [TabGroup("Component"), SerializeField] private Image PostGauge;
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI EntityName;
    [TabGroup("Component"), SerializeField] private Animator mAnimator;
    [Title("HpBar")]
    [TabGroup("Option"), SerializeField] private float runtime;
    [TabGroup("Option"), SerializeField] private AnimationCurve curve;
    [TabGroup("Option"), SerializeField] private float mCloseAniTime = 0.34f;
    #endregion
    #region Value
    protected CharacterUI m_Target;
    private CoroutineWrapper wrapper;
    #endregion

    #region Event
    //Popup Event
    protected override void OnInitData()
    {
        base.OnInitData();

        //변수 초기화
        wrapper = new CoroutineWrapper(this);
    }

    //Unity Event
    private void FixedUpdate()
    {
        if (m_Target)
            transform.localPosition = (m_Target.HPGaugeAt.position + m_Target.HPGaugeOffset).WorldToCanvas(transform.parent as RectTransform);
    }

    //Target Event
    protected virtual void OnRemoveTarget()
    {
        RemoveDataChangeEvent(m_Target.Data.Hp, OnHPChanged);
    }
    protected virtual void OnAddTarget()
    {
        transform.localPosition = (m_Target.HPGaugeAt.position + m_Target.HPGaugeOffset).WorldToCanvas(transform.parent as RectTransform);
        if (PostGauge)
            PostGauge.fillAmount = (float)m_Target.Data.Hp.Value / m_Target.Data.MaxHp;
        if (PreGauge)
            PreGauge.fillAmount = (float)m_Target.Data.Hp.Value / m_Target.Data.MaxHp;

        if (m_Target.Data.EntityType.IsPlayerEntity())
        {
            var n = ClientSessionManager.Instance.UserSessionData.GetUsernameByCharacterType(m_Target.Data.EntityType);
            if (!string.IsNullOrEmpty(n))
                EntityName.text = n;
            else
                EntityName.text = m_Target.Data.EntityType.GetEntityName();
        }
        else
            EntityName.text = m_Target.Data.EntityType.GetEntityName();

        AddDataChangeEvent(m_Target.Data.Hp, OnHPChanged, true);
    }

    //Data Event
    private void OnHPChanged(int hp)
    {
        wrapper.StartSingleton(DecreaseHP(PreGauge, (float)hp / m_Target.Data.MaxHp)).SetOnComplete((bool isComplete) =>
        {
            if (!isComplete)
                return;

            wrapper.StartSingleton(DecreaseHP(PostGauge, PreGauge.fillAmount));
        });
    }
    #endregion
    #region Function
    //Public
    public void Open()
    {
        gameObject.SetActive(true);
        mAnimator.Play("Open");
    }
    public void Close()
    {
        StartCoroutine(CloseHPCor());
    }
    /// <summary>
    /// 누구의 HP를 표시할지 설정합니다.
    /// </summary>
    /// <param name="trackTarget"></param>
    /// <param name="offset"></param>
    /// <param name="data"></param>
    public void SetTarget(CharacterUI _tar)
    {
        if (m_Target)
            OnRemoveTarget();
        m_Target = _tar;
        if (m_Target)
            OnAddTarget();
    }

    //Coroutine
    private IEnumerator DecreaseHP(Image target, float ratio)
    {
        if (target == null)
            yield break;

        float t = 0;
        float defaultratio = target.fillAmount;
        while (t < runtime)
        {
            target.fillAmount = Mathf.Lerp(defaultratio, ratio, curve.Evaluate(t / runtime));
            t += Time.deltaTime;
            yield return null;
        }

        target.fillAmount = ratio;
    }
    private IEnumerator CloseHPCor()
    {
        mAnimator.Play("Close");
        yield return new WaitForSeconds(mCloseAniTime);
        SetTarget(null);
        gameObject.SetActive(false);
    }
    #endregion
}