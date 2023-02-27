using Network.Client;
using Network.Common;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;

namespace CKC2022
{

    [Serializable]
    public class HitMeshVertexColorSetter
    {
        [SerializeField]
        private AnimationCurve ColorLerpCurve;
        
        [SerializeField]
        private float runtime;
        
        [SerializeField]
        private Color activeColor;

        [SerializeField]
        private Color inactiveColor;

        private readonly List<Material> targetMaterials = new List<Material>();

        private float Threshold = 0.5f;

        private static readonly string ShaderPropertyName = "_HitColor";

        private CoroutineWrapper wrapper;

        public void Initialize(in MonoBehaviour root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                targetMaterials.AddRange(renderer.materials);
            }

            wrapper = new CoroutineWrapper(root);
        }

        public void RunHitEffect()
        {
            wrapper.StartSingleton(HitEffect());
        }

        IEnumerator HitEffect()
        {
            float t = 0;
            while (t < runtime)
            {
                var value = Mathf.RoundToInt(ColorLerpCurve.Evaluate(t / runtime));
                SetColor(value > Threshold ? activeColor : inactiveColor);
                
                t += Time.deltaTime;
                yield return null;
            }

            var last = Mathf.RoundToInt(ColorLerpCurve.Evaluate(1));
            SetColor(last > Threshold ? activeColor : inactiveColor);
        }

        private void SetColor(in Color color)
        {
            foreach(var mat in targetMaterials)
            {
                mat.SetColor(ShaderPropertyName, color);
            }
        }

        public void Release()
        {
            SetColor(inactiveColor);
        }

        public void Setup()
        {
            ColorUtility.TryParseHtmlString("#414141", out activeColor);
            ColorUtility.TryParseHtmlString("#FFFFFF", out inactiveColor);

            runtime = 0.5f;
            

            ColorLerpCurve = new AnimationCurve();
            ColorLerpCurve.AddKey(new Keyframe(0.0f, 1.0f, Mathf.Infinity, Mathf.Infinity));
            ColorLerpCurve.AddKey(new Keyframe(0.2f, 0.0f, Mathf.Infinity, Mathf.Infinity));
            ColorLerpCurve.AddKey(new Keyframe(0.4f, 1.0f, Mathf.Infinity, Mathf.Infinity));
            ColorLerpCurve.AddKey(new Keyframe(0.6f, 0.0f, Mathf.Infinity, Mathf.Infinity));
            ColorLerpCurve.AddKey(new Keyframe(0.8f, 1.0f, Mathf.Infinity, Mathf.Infinity));
            ColorLerpCurve.AddKey(new Keyframe(1.0f, 0.0f, Mathf.Infinity, Mathf.Infinity));
        }
        /*
         *
        //UnityEditor.AnimationCurveWrapperJSON:{"curve":{"serializedVersion":"2",
        "m_Curve":[
        {"serializedVersion":"3","time":0.0,"value":1.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.0,"outWeight":0.3333333432674408},
        {"serializedVersion":"3","time":0.2,"value":0.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.3333333432674408,"outWeight":0.3333333432674408},
        {"serializedVersion":"3","time":0.4,"value":1.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.3333333432674408,"outWeight":0.3333333432674408},
        {"serializedVersion":"3","time":0.6,"value":0.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.3333333432674408,"outWeight":0.3333333432674408},
        {"serializedVersion":"3","time":0.8,"value":1.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.3333333432674408,"outWeight":0.3333333432674408},
        {"serializedVersion":"3","time":1.0,"value":0.0,"inSlope":Infinity,"outSlope":Infinity,"tangentMode":103,"weightedMode":0,"inWeight":0.3333333432674408,"outWeight":0.0}],"m_PreInfinity":2,"m_PostInfinity":2,"m_RotationOrder":4}}
         **/
    }

    [RequireComponent(typeof(ReplicatedEntityData))]
    public class ReplicatedEntityHitEffector : MonoBehaviour
    {
        [SerializeField]
        private ReplicatedEntityData replicatedEntityData;

        [SerializeField]
        private HitMeshVertexColorSetter hitMeshVertexColorSetter;

        private void Awake()
        {
            if(replicatedEntityData == null)
                replicatedEntityData = GetComponent<ReplicatedEntityData>();

            if (hitMeshVertexColorSetter != null)
                hitMeshVertexColorSetter.Initialize(this);

            replicatedEntityData.OnHitAction += ReplicatedEntityData_OnHitAction;
        }

        private void ReplicatedEntityData_OnHitAction(ReplicatedDetectedInfo info)
        {
            //vertex shading
            hitMeshVertexColorSetter?.RunHitEffect();

            //Effect and sound
            if (!ItemManager.TryGetConfig(info.DetectorInfo.detectorType, out var config))
                return;

            RunEffect(config);

            RunReactionSound(config);

            RunSpecificSound(replicatedEntityData.EntityType);
        }

        private void RunEffect(in Network.Data.WeaponConfiguration config)
        {
            if (config.REACTION_EFFECT == null)
                return;

            //effect
            var instance = PoolManager.SpawnObject(config.REACTION_EFFECT, transform.position, Quaternion.identity);

            if (!instance.TryGetComponent<EffectAutoRelease>(out var autoRelease))
                instance.AddComponent<EffectAutoRelease>();
        }

        private void RunReactionSound(in Network.Data.WeaponConfiguration config)
        {
            if (config.REACTION_SOUND_CODE == SoundType.None)
                return;

            GameSoundManager.Play(config.REACTION_SOUND_CODE, new SoundPlayData(transform.position));
        }

        private void RunSpecificSound(in EntityType entityType)
        {
            var soundCode = entityType switch
            {
                EntityType.kMagicBore => SoundType.MagicBore_Hit,
                EntityType.kWisp => SoundType.Wisp_Hit,
                EntityType.kSpirit => SoundType.MagicBore_Hit,

                _ => SoundType.Common_Hit
            };

            GameSoundManager.Play(soundCode, new SoundPlayData(transform.position));
        }

        private void OnDisable()
        {
            hitMeshVertexColorSetter?.Release();
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void TestRun()
        {
            hitMeshVertexColorSetter?.RunHitEffect();
        }
        
        [Sirenix.OdinInspector.Button]
        public void TestSetup()
        {
            var meshFilter = GetComponentsInChildren<MeshFilter>();
            var skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();

            Selection.objects = meshFilter.Select(filter => filter.sharedMesh)
                .Union(skinnedMeshRenderer.Select(skin => skin.sharedMesh)).ToArray();
        }

        [Sirenix.OdinInspector.Button]
        public void TestSetUpField()
        {
            hitMeshVertexColorSetter?.Setup();
        }
#endif
    }
}