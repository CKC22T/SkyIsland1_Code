using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookOpener : MonoBehaviour
{
    public static BookOpener Instance { get; private set; }
    #region Inspector
    [TabGroup("Component"), SerializeField] private Animator mAnimator;
    [TabGroup("Component"), SerializeField] private GameObject mTitleCinematic;
    [TabGroup("Component"), SerializeField] private Material mLeftPage;
    [TabGroup("Component"), SerializeField] private Material mRightPage;
    [TabGroup("Component"), SerializeField] private GameObject mLeftObject;
    [TabGroup("Component"), SerializeField] private GameObject mRightObject;
    [TabGroup("Component"), SerializeField] private GameObject mBookUI;
    [TabGroup("Option"), SerializeField] private float mOpenTime = 1.6f;
    [TabGroup("Option"), SerializeField] private float mTurnTime = 2.4f;
    [TabGroup("Component"), SerializeField] private Texture2D[] mPageTex;
    #endregion
    #region Get,Set
    /// <summary>
    /// 책이 현재 열려있는 상태인지
    /// </summary>
    public bool IsOpened { get; private set; }
    /// <summary>
    /// 현재 뭔가 애니메이션이 재생중인 상태인지
    /// </summary>
    public bool IsDelay { get => mDelayCor != null; }
    #endregion
    #region Value
    private Action mOnEnd;
    private Coroutine mDelayCor;
    private int mPage;
    #endregion

    #region Event
    private void Awake()
    {
        Instance = this;

        mLeftObject.SetActive(false);
        mRightObject.SetActive(false);
    }

    //Animation Event
    public void OnTurnStart()
    {
        mRightObject.SetActive(true);
    }
    public void OnTurnEnd()
    {
        mLeftObject.SetActive(false);
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 책을 펼칩니다.
    /// </summary>
    /// <param name="_onEnd"></param>
    public void Open(Action _onEnd)
    {
        IsOpened = true;
        mOnEnd = _onEnd;

        mBookUI.SetActive(true);
        mPage = 0;
        mTitleCinematic.SetActive(true);
        mLeftPage.SetTexture("_BaseColorTex", mPageTex[0]);
        mAnimator.Play("Open");
        mDelayCor = StartCoroutine(DelayCor(mOpenTime));
        ++mPage;
    }
    /// <summary>
    /// 다음 페이지로 넘어간다.
    /// </summary>
    public void NextPage()
    {
        if (IsDelay)
            return; //아직 넘기는중인 경우

        if (mPage == mPageTex.Length)
        {
            mOnEnd?.Invoke();
            return; //마지막 페이지인 경우
        }

        mLeftObject.SetActive(1 < mPage);
        mRightObject.SetActive(false);
        mLeftPage.SetTexture("_BaseColorTex", mPageTex[mPage - 1]);
        mRightPage.SetTexture("_BaseColorTex", mPageTex[mPage]);
        mAnimator.Play("Turn", 0, 0);
        mAnimator.speed = 1;
        mDelayCor = StartCoroutine(DelayCor(mTurnTime));
        ++mPage;
    }
    /// <summary>
    /// 더 빠르게 만듭니다.
    /// </summary>
    public void Fast()
    {
        mAnimator.speed += 5.0f;
    }
    /// <summary>
    /// 바로 완료 이벤트를 호출합니다.
    /// </summary>
    public void Skip()
    {
        mOnEnd?.Invoke();
    }

    private IEnumerator DelayCor(float _delay)
    {
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime * mAnimator.speed;
            yield return null;

            if (_delay <= timer)
                break;
        }
        mDelayCor = null;
    }
    #endregion
}
