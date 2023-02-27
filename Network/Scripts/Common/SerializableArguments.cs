using Network;
using Network.Client;
using Network.Server;
using Network.Common;

[System.Serializable]
public struct Tuple<T1, T2>
{
    public T1 Item1;
    public T2 Item2;
}

[System.Serializable]
public struct Tuple<T1, T2, T3>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
}

[System.Serializable]
public struct Tuple<T1, T2, T3, T4>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
}

[System.Serializable]
public struct Tuple<T1, T2, T3, T4, T5>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
}