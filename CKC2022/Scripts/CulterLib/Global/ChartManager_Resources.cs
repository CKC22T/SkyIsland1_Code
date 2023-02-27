using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace CulterLib.Global.Chart
{
    public class ChartManager_Resources : ChartManager
    {
        protected override void OnLoad(string _chartName, Action<Dictionary<string, string>> _onEnd)
        {
            var chart = new Dictionary<string, string>();
            var asset = Resources.Load<TextAsset>($"Table\\{_chartName}");
            if (asset != null)
            {
                var json = Json.Deserialize(asset.text) as Dictionary<string, object>;
                foreach (var v in json)
                    chart.Add(v.Key, v.Value as string);
            }
            _onEnd?.Invoke(chart);
        }
    }
}