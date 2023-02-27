using System;
using UnityEngine;

[Serializable]
public class Readonly<T>
{
    [field: SerializeField] public T Value { get; private set; }
}
