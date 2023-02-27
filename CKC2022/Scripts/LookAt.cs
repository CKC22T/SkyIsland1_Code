using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private void FixedUpdate()
    {
        transform.LookAt(target);
    }
}
