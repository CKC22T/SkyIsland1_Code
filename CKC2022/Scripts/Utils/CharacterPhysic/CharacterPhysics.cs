using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Utils
{
    /// <summary>
    /// 캐릭터가 공중에 떠있는지&떠있는 이유
    /// </summary>
    public enum ECharacterState
    {
        /// <summary>
        /// 지상에 있는중 - 이동 & 점프 가능
        /// </summary>
        Ground,
        /// <summary>
        /// 미끄러지는중 - 행동 불가
        /// </summary>
        Slide,
        /// <summary>
        /// 공중에서 떨어지는중 - 이동 & 점프 가능
        /// </summary>
        Fall,
        /// <summary>
        /// 점프중 - 이동 & 점프 가능
        /// </summary>
        Jump,
        /// <summary>
        /// 넉백당함 - 행동 불가
        /// </summary>
        Knockback,
    }

    public class CharacterPhysics : MonoBehaviour
    {
        #region Type
        /// <summary>
        /// 인접한 바닥에 대한 데이터
        /// </summary>
        private struct SGroundData
        {
            public Vector3 position;
            public Vector3 normal;

            public SGroundData(Collision _col)
            {
                position = Vector3.zero;
                normal = Vector3.zero;
                foreach (var v in _col.contacts)
                {
                    position += v.point;
                    normal += v.normal;
                }
                position /= _col.contactCount;
                normal.Normalize();
            }
        }
        #endregion

        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("Rigidbody")] private Rigidbody m_Rigidbody;
        [SerializeField, TabGroup("Component"), LabelText("Collider")] private Collider m_Collider;
        [SerializeField, TabGroup("Component"), LabelText("발 기준점")] private Transform m_FootTransform;

        [SerializeField, TabGroup("Physics"), LabelText("기본 머테리얼")] private PhysicMaterial m_DefaultPhysicsMaterial;
        [SerializeField, TabGroup("Physics"), LabelText("이동시 머테리얼")] private PhysicMaterial m_MovePhysicsMaterial;

        [SerializeField, TabGroup("Option"), LabelText("자동 초기화")] private bool m_IsAutoInit = true;
        [SerializeField, TabGroup("Option"), LabelText("중력 배율")] private float m_GravityScale = 1.0f;
        [SerializeField, TabGroup("Option"), LabelText("최대 이동속도")] private float m_MoveSpeed;
        [SerializeField, TabGroup("Option"), LabelText("0->최대이속 시간")] private float m_MoveMaxSec;
        [SerializeField, TabGroup("Option"), LabelText("최대이속->0 시간")] private float m_MoveMinSec;
        [SerializeField, TabGroup("Option"), LabelText("공중에서 이속 변경시간")] private float m_FlyMoveChangeSec;
        [SerializeField, TabGroup("Option"), LabelText("미끄러지는 각도")] private float m_SlideAngle;
        [SerializeField, TabGroup("Option"), LabelText("점프 세기")] private float m_JumpPower;
        [SerializeField, TabGroup("Option"), LabelText("넉백 기절 시간")] private float m_KnockbackStunTime;
        [SerializeField, TabGroup("Option"), LabelText("넉백 커브 시간")] private float m_KnockbackCurveTime;
        [SerializeField, TabGroup("Option"), LabelText("넉백 지상속도 커브")] private AnimationCurve m_KnockbackCurve;
        #endregion
        #region Get,Set
        //Component
        /// <summary>
        /// 캐릭터 Rigidbody, 왠만하면 접근하지 말것!!!
        /// </summary>
        public Rigidbody TargetRigidbody { get => m_Rigidbody; }
        /// <summary>
        /// 캐릭터 Collider, 왠만하면 접근하지 말것!!!
        /// </summary>
        public Collider TargetCollider { get => m_Collider; }
        //State
        /// <summary>
        /// 캐릭터 상태
        /// </summary>
        public INotifiable<ECharacterState> State { get => m_State; }
        /// <summary>
        /// 캐릭터가 현재 땅위에 있는지
        /// </summary>
        public bool IsGround { get => 0 < m_GroundData.Count; }
        /// <summary>
        /// 캐릭터가 이동이 가능한 상태인지
        /// </summary>
        public bool IsMoveEnable { get => m_State.Value == ECharacterState.Ground || m_State.Value == ECharacterState.Fall || m_State.Value == ECharacterState.Jump; }
        /// <summary>
        /// 캐릭터가 점프가 가능한 상태인지
        /// </summary>
        public bool IsJumpEnable { get => m_State.Value == ECharacterState.Ground || m_State.Value == ECharacterState.Fall; }
        //Option
        /// <summary>
        /// 이동속도
        /// </summary>
        public readonly Notifier<float> MoveSpeed = new Notifier<float>();
        /// <summary>
        /// 점프 세기
        /// </summary>
        public readonly Notifier<float> JumpPower = new Notifier<float>();
        #endregion
        #region Value
        private Notifier<ECharacterState> m_State = new Notifier<ECharacterState>(ECharacterState.Fall);
        private Dictionary<Collider, SGroundData> m_GroundData = new Dictionary<Collider, SGroundData>();   //현재 인접한 바닥 콜라이더 및 관련 정보
        [SerializeField, ReadOnly] protected bool m_IsMoved;                                   //이번 프레임에 이동을 했는지 여부
        [SerializeField, ReadOnly] protected Vector2 m_MoveVelocity;                           //목표 이동방향
        protected float m_MoveEndTimer;                             //이 시간 이상 지나면 강제로 멈추도록 함
        protected float m_KnockbackTimer;                           //넉백 최소 시간 (바닥에서 넉백시)
        protected Vector3 m_KnockbackPower;                         //넉백 세기 (바닥에서 넉백시)
        #endregion

        #region Event
        public void Initialize()
        {
            //변수 초기화
            MoveSpeed.Value = m_MoveSpeed;
            JumpPower.Value = m_JumpPower;
        }
        public void Initialize(float _moveSpd, float _jumpPower)
        {
            //변수 초기화
            MoveSpeed.Value = _moveSpd;
            JumpPower.Value = _jumpPower;
        }

        //Unity Event
        private void Awake()
        {
            if (m_IsAutoInit)
                Initialize();
        }
        protected virtual void FixedUpdate()
        {
            //State 업데이트
            if (ECharacterState.Fall <= m_State.Value && m_State.Value < ECharacterState.Knockback)
                m_State.Value = (m_GroundData.Count == 0) ? m_State.Value : ECharacterState.Ground;
            else if (m_State.Value == ECharacterState.Knockback && m_KnockbackStunTime <= m_KnockbackTimer && IsGround)
                m_State.Value = (m_GroundData.Count == 0) ? m_State.Value : ECharacterState.Ground;
            if (m_State.Value <= ECharacterState.Slide)
            {
                if (m_GroundData.Count == 0)
                    m_State.Value = ECharacterState.Fall;
                else
                {
                    var normal = Vector3.zero;
                    foreach (var v in m_GroundData.Values)
                        normal += v.normal;
                    m_State.Value = (Vector3.Angle(Vector3.up, normal.normalized) < m_SlideAngle) ? ECharacterState.Ground : ECharacterState.Slide;
                    m_Rigidbody.velocity = m_Rigidbody.velocity.AdaptY(Mathf.Min(0, m_Rigidbody.velocity.y));
                }
            }

            //이동 업데이트
            var physicsMat = m_DefaultPhysicsMaterial;
            if (State.Value == ECharacterState.Knockback)
            {   //넉백 업데이트
                physicsMat = m_MovePhysicsMaterial;
                m_KnockbackTimer += Time.fixedDeltaTime;
                if (m_KnockbackTimer <= m_KnockbackCurveTime)
                    m_Rigidbody.velocity = (m_KnockbackPower * m_KnockbackCurve.Evaluate(m_KnockbackTimer / m_KnockbackCurveTime));
            }
            else if (IsMoveEnable)
            {   //이동 업데이트
                if (m_IsMoved)
                {   //이동했을 경우 - 가속
                    float changeSec = IsGround ? m_MoveMaxSec : m_FlyMoveChangeSec;     //몇초에 걸쳐 이동속도를 변경할지 구한다.
                    m_Rigidbody.velocity = GetMoveVelocity(m_MoveVelocity * MoveSpeed.Value, changeSec);                               //실제로 변경시킨다.
                    physicsMat = m_MovePhysicsMaterial;                                 //이동 재질로 변경한다.
                }
                else
                {   //이동하지 않은 경우 - 바닥에 있는경우만 서서히 이동속도를 줄임
                    if (IsGround)
                    {
                        if (0 < m_MoveEndTimer)
                        {
                            m_Rigidbody.velocity = GetMoveVelocity(Vector2.zero, m_MoveMinSec);
                            m_MoveEndTimer -= Time.fixedDeltaTime;
                            physicsMat = m_MovePhysicsMaterial;
                        }
                        else
                        {
                            m_Rigidbody.velocity = new Vector3(0, TargetRigidbody.velocity.y, 0);
                            physicsMat = m_DefaultPhysicsMaterial;
                        }
                    }
                    else
                        physicsMat = m_MovePhysicsMaterial;  
                }
            }

            //끝!
            m_Collider.sharedMaterial = physicsMat;                                                 //PhysicsMaterial 적용
            m_Rigidbody.velocity += Physics.gravity * (m_GravityScale - 1) * Time.fixedDeltaTime;   //추가중력 적용
            m_IsMoved = false;
        }
        private void OnCollisionEnter(Collision _col)
        {
            SGroundData data = new SGroundData(_col);
            if (data.position.y < m_FootTransform.position.y)
                SetGround(_col.collider, data);
        }
        private void OnCollisionStay(Collision _col)
        {
            bool isNotJumping = m_State.Value != ECharacterState.Jump || TargetRigidbody.velocity.y <= 0;
            if (isNotJumping)
            {
                SGroundData data = new SGroundData(_col);
                if (data.position.y < m_FootTransform.position.y)
                    SetGround(_col.collider, data);
                else
                    RemoveGround(_col.collider);
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            RemoveGround(collision.collider);
        }
        #endregion
        #region Function - Public
        //Public
        /// <summary>
        /// 해당 방향으로 이동합니다.
        /// </summary>
        /// <param name="vec">이동할 속도</param>
        /// <param name="isNow">즉시 해당 속도로 변할지</param>
        public void Move(Vector2 vec, bool isNow = false)
        {
            if (vec.sqrMagnitude <= float.Epsilon)
                return;

            vec.Normalize();
            m_IsMoved = true;                  //이번 프레임에 이동했다고 한다.
            m_MoveVelocity = MoveSpeed.Value * vec;     //이동 방향/세기를 기록한다.
            m_MoveEndTimer = m_MoveMinSec;              //이동 타이머를 초기화한다.

            if (isNow)
                m_Rigidbody.velocity = new Vector3(vec.x, m_Rigidbody.velocity.y, vec.y);
        }
        /// <summary>
        /// 점프합니다.
        /// </summary>
        /// <param name="power">점프 세기</param>
        public void Jump(bool _isNow = false)
        {
            if (IsJumpEnable || _isNow)
            {
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, JumpPower.Value, m_Rigidbody.velocity.z);    //velocity를 설정한다.
                m_State.Value = ECharacterState.Jump;
                ClearGround();
            }
        }
        /// <summary>
        /// 넉백시킵니다.
        /// </summary>
        public void Knockback(Vector3 _force)
        {
            m_Rigidbody.velocity = _force;
            m_State.Value = ECharacterState.Knockback;
            m_KnockbackTimer = 0;
            m_KnockbackPower = _force;
            ClearGround();
        }
        /// <summary>
        /// 해당 방향으로 순간이동합니다.
        /// </summary>
        public void Teleport(Vector3 teleportPosition)
        {
            TargetRigidbody.MovePosition(teleportPosition);
        }
        #endregion
        #region Function - Private
        /// <summary>
        /// 이동 속도를 changeSec에 걸쳐 변경합니다.
        /// </summary>
        /// <param name="move">목표 이동방향/속도</param>
        /// <param name="changeSec">변경에 걸리는 시간</param>
        private Vector3 GetMoveVelocity(Vector2 move, float changeSec)
        {
            if (changeSec != 0)
            {   //변경에 걸리는 시간이 0이 아닌 경우 changeSec에 걸쳐 변경한다.
                float velocityX = Mathf.Lerp(m_Rigidbody.velocity.x, move.x, (MoveSpeed.Value / Mathf.Abs(m_Rigidbody.velocity.x - move.x)) * Time.deltaTime / changeSec);
                float velocityY = Mathf.Lerp(m_Rigidbody.velocity.z, move.y, (MoveSpeed.Value / Mathf.Abs(m_Rigidbody.velocity.z - move.y)) * Time.deltaTime / changeSec);
                return new Vector3(velocityX, m_Rigidbody.velocity.y, velocityY);
            }
            else
            {   //0인 경우는 즉시 변경한다.
                return new Vector3(move.x, m_Rigidbody.velocity.y, move.y);
            }
        }
        /// <summary>
        /// 밟고있는 바닥을 추가합니다.
        /// </summary>
        /// <param name="col">콜라이더</param>
        private void SetGround(Collider _col, SGroundData _data)
        {
            if (!m_GroundData.ContainsKey(_col))
                m_GroundData.Add(_col, _data);
            else
                m_GroundData[_col] = _data;
        }
        /// <summary>
        /// 밟고 있었던 바닥을 제거합니다.
        /// </summary>
        /// <param name="col"></param>
        private void RemoveGround(Collider col)
        {
            m_GroundData.Remove(col);
        }
        /// <summary>
        /// 현재 밟고있는 것으로 등록된 바닥을 전부 제거합니다.
        /// </summary>
        private void ClearGround()
        {
            m_GroundData.Clear();
        }
        #endregion
    }
}