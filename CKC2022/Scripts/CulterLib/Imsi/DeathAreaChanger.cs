using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAreaChanger : MonoBehaviour
{
    [Sirenix.OdinInspector.Button("Change")]
    public void Change()
    {
        var scale = transform.localScale;
        var bc = GetComponent<BoxCollider>();
        scale.x *= bc.size.x;
        scale.y *= bc.size.y;
        scale.z *= bc.size.z;

        transform.localScale = scale;
        bc.size = Vector3.one;
    }
}
