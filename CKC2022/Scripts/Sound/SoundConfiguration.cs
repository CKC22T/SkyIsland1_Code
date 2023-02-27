using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Utils;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CKC2022
{
    public enum SoundType
    {
        None = 0,

        Title_Background = 001, //	타이틀 씬에서 출력
        Game_Background = 002,  //	인게임에서 출력
        Lobby_BackGround = 003, //	로비 씬에서 출력
        Boss_BackGround = 004,  //	보스전 시작시 출력
        End_BackGround = 005,   //	엔딩 화면에서 출력


        Walk_Dirt = 101,    //	플레이어가 땅 위를 걸을 때 출력
        Walk_Stone = 102,   //	플레이어가 돌 위를 걸을 때 출력
        Player_Respawn = 103,   //	플레이어 부활시 출력
        Player_Dead = 104,  //	플레이어 사망시 출력
        Player_GetWeapon = 105, //	플레이어 무기 획득시 출력

        MagicBore_Attack = 121, //	매직 보어 돌진시 출력
        MagicBore_Dead = 122,   //	매직보어 사망시 출력
        MagicBore_Hit = 123,    //	매직보어 타격시 출력

        Wisp_Attack = 131,  //	위습 공격시 출력
        Wisp_Dead = 132,    //	위습 사망시 출력
        Wisp_Hit = 133,	//	위습 타격시 출력

        Boss_Attack_01 = 141,   //	보스 패턴 1 사용시 출력
        Boss_Attack_02 = 142,   //	보스 패턴 2 사용시 출력
        Boss_Attack_03 = 143,   //	보스 패턴 3 사용시 출력
        Boss_PageChange = 144,  //	페이지 전환 시 출력
        Boss_Dead = 145,    //	보스 사망시 출력
        Boss_Respawn = 146, //	보스 등장시 출력
        Boss_Walk = 147,    //	보스 이동시 출력
        Common_Hit = 151,	//	공용 - 피격시 출력

        Weapon_Basic = 201, //	기본 무기 사용시 출력
        Weapon_Basic_Blow = 202,    //	기본 무기 타격시 출력
        Weapon_Heal = 203,  //	힐 무기 사용시 출력
        Weapon_Heal_Blow = 204, //	힐 무기 타격시 출력
        Weapon_Explosion = 205, //	폭발 무기 사용시 출력
        Weapon_Explosion_Blow = 206,    //	폭발 무기 타격시 출력
        Weapon_Bolt = 207,  //	감전 무기 사용시 출력
        Weapon_Bolt_Blow = 208, //	감전 무기 타격시 출력
        Weapon_Laser = 209, //	레이저 무기 타격시 출력
        Weapon_Sword = 210, //	칼 무기 사용시 출력
        Weapon_Sword_Blow = 211,	//	칼 무기 타격시 출력
        Weapon_Key = 212,   //	Key of Wisdom 사용시 출력
        Weapon_Key_Blow = 213,	//	Key of Wisdom 타격시 출력

        CheckPoint_On = 301,    //	체크포인트 활성화 시 출력
        CheckPoint_Destroy = 302,   //	체크포인트 퇴장시 출력
        Bridge_On = 303,    //	부유석 다리 생성 시 출력

        bonfire_ABM = 401,  //	로비 씬 장작 위치에서 방사
        Forest_AMB = 402,   //	인게임에서 항상 출력
        Waterfall_AMB = 403,    //	폭포 근처에서 방사
        Wind_AMB = 404, //	?


        UI_Button = 501,    //	플레이어가 버튼 클릭 시 출력
        UI_InGame = 502,    //	인게임 특정 UI출력시 같이 출력
        UI_Lobby_Button = 503,  //	로비에서 캐릭터 선택시 출력
        UI_turn = 504,	//	타이틀 화면 전환시 사용
    }

    
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "SoundConfigurationData", menuName = "SoundData", order = 0)]
#endif

    public class SoundConfiguration : ScriptableObject
    {
        public SoundType type;
        public List<AudioClip> clips;

        public AudioClip clip { get => clips.GetRandom(); }

        [Range(0f, 1f)]
        public float volume = 1f;

        [Range(0.1f, 1.5f)]
        public float pitch = 1f;

        [Range(0.01f, 10f)]
        public float pitchRandomMultiplier = 1f;

        public bool loop = false;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void SetupByName()
        {
            var code = name.Substring(1,3);
            var additionalValue = 0;
            if(name.Length == 7)
            {
                additionalValue = int.Parse(name.Substring(6, 1));
            }

            this.type = (SoundType)(int.Parse(code) + additionalValue);

            code = ((int)type).ToString("D3");
            
            clips = new List<AudioClip>();

            var allClipsGuid = AssetDatabase.FindAssets("t: AudioClip");
            var filename = string.Empty;
            foreach (var guid in allClipsGuid)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));

                if (clip.name.Contains(code))
                {
                    clips.Add(clip);
                    filename = clip.name;
                }
            }

            if (filename != string.Empty)
            {
                string assetPath = AssetDatabase.GetAssetPath(this.GetInstanceID());
                AssetDatabase.RenameAsset(assetPath, filename);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}