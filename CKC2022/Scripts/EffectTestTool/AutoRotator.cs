using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    [SerializeField]
    private Transform Target;

    [SerializeField]
    private float speed;


    void Update()
    {
        Target.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
