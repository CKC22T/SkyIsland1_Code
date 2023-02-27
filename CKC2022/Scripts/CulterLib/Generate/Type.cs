using Sirenix.OdinInspector;
using System;
using UnityEngine;
using CulterLib.Types;

namespace CKC2022.GameData.Data
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable] public partial struct SMonsterOtherStat
    {
        /// <summary>
        /// 스탯 ID
        /// </summary>
        public string Id { get => id; set => id = value; }
        /// <summary>
        /// 스탯 수치
        /// </summary>
        public int V { get => v; set => v = value; }
    
        [SerializeField] private string id;
        [SerializeField] private int v;
    
        public SMonsterOtherStat(string _id, int _v)
        {
            id = _id;
            v = _v;
        }
        public SMonsterOtherStat ChangeId(string _change)
        {
            var s = this;
            s.Id = _change;
            return s;
        }
        public SMonsterOtherStat ChangeV(int _change)
        {
            var s = this;
            s.V = _change;
            return s;
        }
    }
    /// <summary>
    /// 몬스터테이블
    /// </summary>
    [Serializable] public partial class MonsterTable : IName
    {
        /// <summary>
        /// 몬스터 이름
        /// </summary>
        public TranslateText Name { get => name; set => name = value; }
        /// <summary>
        /// 체력
        /// </summary>
        public int Hp { get => hp; set => hp = value; }
        /// <summary>
        /// 공격력
        /// </summary>
        public int Atk { get => atk; set => atk = value; }
        /// <summary>
        /// 몬스터들의 특수 스탯
        /// </summary>
        public SMonsterOtherStat[] Otherstat { get => otherstat; set => otherstat = value; }
    
        [SerializeField] private TranslateText name;
        [SerializeField] private int hp;
        [SerializeField] private int atk;
        [SerializeField] private SMonsterOtherStat[] otherstat;
    
        public MonsterTable(TranslateText _name, int _hp, int _atk, SMonsterOtherStat[] _otherstat)
        {
            name = _name;
            hp = _hp;
            atk = _atk;
            otherstat = _otherstat;
        }
    }
}
