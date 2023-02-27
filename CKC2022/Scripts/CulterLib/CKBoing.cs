using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CKBoing : MonoBehaviour
{
    #region Type
    [System.Serializable]
    private class BoneData
    {
        public Transform tr;
        public Vector3 oriLPos;
        public Quaternion oriLRot;
        public Vector3 lastedWPos;

        public BoneData(Transform _tr)
        {
            tr = _tr;
            oriLRot = tr.localRotation;
            lastedWPos = _tr.position;
            oriLPos = tr.localPosition;
        }
    }
    #endregion

    #region Inspector
    [TabGroup("Option"), SerializeField] private Vector3 m_AddAngle;
    [TabGroup("Option"), MinMaxSlider(0.0f, 10.0f, true), SerializeField] private Vector2 m_SpdRange = new Vector2(0.01f, 10.0f);
    [TabGroup("Option"), Range(0, 180), SerializeField] private float m_AngleLimit = 60;
    [TabGroup("Option"), SerializeField] private float m_AngleDelay = 1.0f;
    [TabGroup("Option"), MinMaxSlider(0.0f, 1.0f, true), SerializeField] private Vector2 m_WeightRange = new Vector2(0.0f, 1.0f);
    [TabGroup("Option"), SerializeField] private AnimationCurve m_WeightCurve;
    [TabGroup("Option"), SerializeField] private AnimationCurve m_VelocityToLerpFactor;
    [TabGroup("Option"), SerializeField] private float m_StretchFactor = 1.0f;
    [TabGroup("Option"), SerializeField] private float m_StretchIgnoreTime = 1.0f;
    #endregion
    #region Value
    private BoneData[] m_BoneDatas;
    private float Uptime;
    #endregion

    #region Event
    private void Start()
    {
        //Bones 초기화
        List<BoneData> boneList = new List<BoneData>();
        Transform tr = transform;
        while (tr != null)
        {
            boneList.Add(new BoneData(tr));
            tr = (0 < tr.childCount) ? tr.GetChild(0) : null;
        }
        m_BoneDatas = boneList.ToArray();
    }

    private void OnEnable()
    {
        Uptime = Time.time;
    }


    private void FixedUpdate()
    {
        for (int i = 0; i < m_BoneDatas.Length; ++i)
        {
            //임시변수
            var curdata = m_BoneDatas[i];
            var curlerp = i / (m_BoneDatas.Length - 1.0f);
            var movevec = curdata.tr.position - curdata.lastedWPos;

            var movespd = movevec.magnitude / Time.deltaTime;
            var moverot = GetLookRotation(movevec) * Quaternion.Euler(m_AddAngle);

            var tarrotmin = curdata.tr.parent.rotation * curdata.oriLRot;
            var tarrotmax = Quaternion.Lerp(tarrotmin, moverot, m_AngleLimit / Quaternion.Angle(tarrotmin, moverot));
            var tarrot = Quaternion.Lerp(tarrotmin, tarrotmax, Mathf.InverseLerp(m_SpdRange.x, m_SpdRange.y, movespd) * Mathf.Lerp(m_WeightRange.x, m_WeightRange.y, m_WeightCurve.Evaluate(curlerp)));
            var tarangle = Quaternion.Angle(curdata.tr.rotation, tarrot);
            var tarlerp = Time.deltaTime / (tarangle / (m_AngleLimit * 2) * m_AngleDelay);

            var currentWPos = curdata.tr.parent.TransformPoint(curdata.oriLPos);
            var stretchLerpFactor = Mathf.Lerp(1, m_VelocityToLerpFactor.Evaluate(movespd.Remap((m_SpdRange.x, m_SpdRange.y), (0, 1))), m_StretchFactor);
            if ((Time.time - Uptime) < 1f)
                stretchLerpFactor = m_StretchIgnoreTime;

            //데이터 처리
            curdata.tr.rotation = Quaternion.Lerp(curdata.tr.rotation, tarrot, tarlerp);
            curdata.tr.position = Vector3.Lerp(curdata.lastedWPos, currentWPos, stretchLerpFactor);

            //데이터 저장
            curdata.lastedWPos = curdata.tr.position;
        }

        static Quaternion GetLookRotation(Vector3 vector)
        {
            if (vector.sqrMagnitude < Vector3.kEpsilon)
                return Quaternion.identity;

            return Quaternion.LookRotation(vector);
        }
    }

    #endregion
}
