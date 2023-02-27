using Sirenix.OdinInspector;
using System;
using UnityEngine;
using CulterLib.Types;
using CulterLib.Global.Chart;
using CulterLib.Global.Load;
using CulterLib.Global.Data;
using System.Collections.Generic;
using CulterLib.Presets;

namespace CKC2022.GameData.Data
{
    public abstract class BaseLoadManager : LoadManager
    {
        protected override ChartManager[] OnNeedConstLoad()
        {
            return new ChartManager[] { GlobalManager.Instance.DataMgr.ServerChart };
        }
        protected override STableLoad[] OnNeedTableLoad()
        {
            return new STableLoad[]
            {
                new STableLoad(GlobalManager.Instance.DataMgr.ServerChart, "MonsterTable", typeof(MonsterTable)),
            };
        }
    }
}
