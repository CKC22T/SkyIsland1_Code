using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class WebJsonParser
{
    public static string ToJson<T>(T instance)
    {
        return JsonConvert.SerializeObject(instance);
    }

    public static T ToInstance<T>(string jsonStream)
    {
        return JsonConvert.DeserializeObject<T>(jsonStream);
    }
}
