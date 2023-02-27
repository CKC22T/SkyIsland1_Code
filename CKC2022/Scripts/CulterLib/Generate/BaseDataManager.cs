using Sirenix.OdinInspector;
using System;
using UnityEngine;
using CulterLib.Types;
using CulterLib.Global.Data;
using System.Collections.Generic;

namespace CKC2022.GameData.Data
{
    public abstract class BaseDataManager : DataManager
    {
        public int vPlayerMaxCount { get; private set; }
        public int vPlayerMaxHP { get; private set; }
        public int vEquipDistance { get; private set; }
        public IReadOnlyList<string> MonsterTableID { get; private set; }
    
        internal override void OnInitConst(Dictionary<string, string> _chart)
        {
            if (_chart.ContainsKey("vPlayerMaxCount"))
                vPlayerMaxCount = int.Parse(_chart["vPlayerMaxCount"]);
            if (_chart.ContainsKey("vPlayerMaxHP"))
                vPlayerMaxHP = int.Parse(_chart["vPlayerMaxHP"]);
            if (_chart.ContainsKey("vEquipDistance"))
                vEquipDistance = int.Parse(_chart["vEquipDistance"]);
        }
        protected override void OnInitData()
        {
            LoadConst(ClientChart, null);
            ClientChart.LoadTable<TranslateText>("TextTable", null);
            ClientChart.LoadTable<MonsterTable>("MonsterTable", null);
            MonsterTableID = TableKeys["MonsterTable"];
        }
    
        public virtual MonsterTable GetMonsterTableData(string _id) => GetTableData<MonsterTable>(_id);
        public virtual bool TryGetMonsterTableData(string _id, out MonsterTable _data) => TryGetTableData(_id, out _data);
    }
}
