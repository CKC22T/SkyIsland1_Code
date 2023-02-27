using CKC2022;
using CulterLib.Utils;
using Network.Client;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CharacterUI : MonoBehaviour
{
    #region Type
    public enum ECharacterUIType
    {
        Default,    //일반 몬스터 UI
        Boss,       //보스 몬스터 UI
        Player,     //플레이어 캐릭터 UI -  Marker가 표시됨
    }
    #endregion

    #region Inspector
    [Title("CharacterUI")]
    [TabGroup("Option"), SerializeField] private ECharacterUIType m_Type;
    [TabGroup("Option"), SerializeField] private Color m_PersonalColor = Color.white;
    [TabGroup("Option"), SerializeField, GUIColor(1.0f, 0.9f, 0.9f)] private bool m_UseHPGauge;
    [TabGroup("Option"), SerializeField, GUIColor(1.0f, 0.9f, 0.9f), ShowIf("m_UseHPGauge")] private Vector3 m_HPGaugeOffset;
    #endregion
    #region Get,Set
    //Common
    /// <summary>
    /// 해당 캐릭터UI의 타입
    /// </summary>
    public ECharacterUIType ChrUIType { get => m_Type; }
    /// <summary>
    /// 해당 캐릭터의 컬러
    /// </summary>
    public Color PersonalColor { get => m_PersonalColor; }

    //HPGauge
    /// <summary>
    /// HPGauge UI 사용 여부
    /// </summary>
    public bool UseHPGauge { get => m_UseHPGauge; }
    /// <summary>
    /// HP게이지 표시할 위치 기준점
    /// </summary>
    public Transform HPGaugeAt { get => holder[PlaceHolder.PlaceType.Neck]; }
    /// <summary>
    /// HP게이지 표시 위치 추가 오프셋
    /// </summary>
    public Vector3 HPGaugeOffset { get => m_HPGaugeOffset; }

    //Other
    /// <summary>
    /// Entity Data가 들어있는곳
    /// </summary>
    public ReplicatedEntityData Data { get => data; }
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private ReplicatedEntityData data;
    [TabGroup("Debug"), SerializeField, ReadOnly] private PlaceHolder holder;
    private Coroutine m_HpCor;
    #endregion

    #region Event
    private void Awake()
    {
        //변수 초기화
        data = transform.parent.GetComponent<ReplicatedEntityData>();
        if (!data || !data.TryGetComponent(out holder))
            return;

        //에러 처리
        if (holder[PlaceHolder.PlaceType.ModelRoot] == null)
            Debug.LogError("Neck is not initialized.");

        //이벤트 초기화
        if (m_Type == ECharacterUIType.Default)
            Data.Hp.OnChanged += () =>
            {   //일반몬스터는 HP가 바뀔때마다 HP바 일정시간동안 표시
                m_HpCor = CoroutineUtil.Change(this, m_HpCor, ShowHpCor());
            };
    }
    private void Start()
    {
        if (m_Type != ECharacterUIType.Default)
            HpUI.Instance?.AddCharacterUI(this);
        if (ChrUIType == ECharacterUIType.Player)
            MarkerUI.Instance?.AddPlayerChrUI(this);
    }
    private void OnEnable()
    {
        if (m_Type != ECharacterUIType.Default)
            HpUI.Instance?.AddCharacterUI(this);
        if (ChrUIType == ECharacterUIType.Player)
            MarkerUI.Instance?.AddPlayerChrUI(this);
    }
    private void OnDisable()
    {
        HpUI.Instance?.RemoveCharacterUI(this);
        if (ChrUIType == ECharacterUIType.Player)
            MarkerUI.Instance?.RemovePlayerChrUI(this);
    }
    private void OnDestroy()
    {
        HpUI.Instance?.RemoveCharacterUI(this);
        if (ChrUIType == ECharacterUIType.Player)
            MarkerUI.Instance?.RemovePlayerChrUI(this);
    }
    #endregion
    #region Function
    /// <summary>
    /// Hp바 일정시간동안 표시하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowHpCor()
    {
        HpUI.Instance?.AddCharacterUI(this);
        yield return new WaitForSeconds(5.0f);
        HpUI.Instance?.RemoveCharacterUI(this);
    }
    #endregion
}