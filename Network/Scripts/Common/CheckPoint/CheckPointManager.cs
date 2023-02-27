using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

[Serializable]
public class SpawnPositionList
{
    public int CheckPointNumber;
    public List<Transform> PositionList;
}

[Serializable]
public class TeleportPositionList
{
    public TeleportPositionType TeleportType;
    public List<Transform> TeleportList;
}

public class CheckPointManager : LocalSingleton<CheckPointManager>
{
    [Sirenix.OdinInspector.Button]
    public void setupCheckPointController()
    {
        CheckPointControllerList = FindObjectsOfType<CheckPointController>().ToList();
        Debug.Log(LogManager.GetLogMessage($"Check point contrller list initialized! Count : {CheckPointControllerList.Count}", NetworkLogType.CheckPointManager));
    }

    [SerializeField] public List<SpawnPositionList> CheckPointSpawnPositionList;
    [SerializeField] public List<CheckPointController> CheckPointControllerList;
    [SerializeField] public List<TeleportPositionList> TeleportPositionList;

    private Dictionary<int, CheckPointController> mCheckPointControllerTable = new();
    private NetworkMode mNetworkMode = NetworkMode.None;

    private bool mIsInitialized = false;

    private CheckPointSystem mMasterCheckPointSystem => ServerSessionManager.Instance.GameGlobalState.GameGlobalState.CheckPointSystem;
    private CheckPointSystem mRemoteCheckPointSystem => ClientSessionManager.Instance.GameGlobalState.GameGlobalState.CheckPointSystem;

    private float mCheckPointInteractSqrDistance = Mathf.Pow(ServerConfiguration.CheckPointInteractDistance, 2);

    private Vector3 mFirstSpawnPosition;

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mIsInitialized)
            return;

        // Initialize First Spawn Position
        var firstPositions = CheckPointSpawnPositionList[0];

        mFirstSpawnPosition = Vector3.zero;

        int count = 0;

        foreach (var t in firstPositions.PositionList)
        {
            mFirstSpawnPosition += t.position;
            count++;
        }

        if (count == 0)
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no spawn point!!!!!!!!!!", NetworkLogType.None, true));
            throw new Exception($"There is no spawn point!!!!!!!!!!");
        }

        mFirstSpawnPosition = mFirstSpawnPosition / count;

        // Initialize network mode
        mIsInitialized = true;

        mNetworkMode = networkMode;

        foreach (var c in CheckPointControllerList)
        {
            mCheckPointControllerTable.Add(c.CheckPointNumber, c);
            c.InitializeByManager(mNetworkMode);
        }

        // Setup as remote
        if (mNetworkMode == NetworkMode.Remote)
        {
            foreach (var c in mCheckPointControllerTable.Values)
            {
                mRemoteCheckPointSystem.BindOnCheckPointWeaponChanged(c.CheckPointNumber, c.TrySetWeaponItem);
            }
        }
    }

    public bool TryGetItemShowerPosition(int checkPointNumber, out Vector3 position)
    {
        if (mCheckPointControllerTable.TryGetValue(checkPointNumber, out var checkPointController))
        {
            position = checkPointController.GetItemShowerPosition();
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public bool TryGetInteractableCheckPointNumber(Vector3 position, out int checkPointNumber)
    {
        checkPointNumber = -1;

        foreach (var c in mCheckPointControllerTable.Values)
        {
            var distanceFromCheckPoint = (c.transform.position - position).sqrMagnitude;

            if (distanceFromCheckPoint < mCheckPointInteractSqrDistance)
            {
                checkPointNumber = c.CheckPointNumber;
                return true;
            }
        }

        return false;
    }

    public bool TryGetPrograss(Vector3 position, int checkPointNumber, out float prograss)
    {
        prograss = 0;

        // First start
        if (checkPointNumber < 0)
        {
            return false;
        }
        else if (checkPointNumber == 0)
        {
            var startPosition = mFirstSpawnPosition;
            var endPosition = CheckPointControllerList[0].transform.position;
            float distanceBetweenCheckPoint = (endPosition - startPosition).sqrMagnitude;
            float distanceFromCheckPoint = (startPosition - position).sqrMagnitude;

            prograss = Mathf.Clamp(distanceFromCheckPoint / distanceBetweenCheckPoint, 0.0f, 1.0f);
            return true;
        }
        else if (checkPointNumber > 0)
        {
            checkPointNumber--;

            var currentPointPosition = CheckPointControllerList[checkPointNumber].transform.position;
            var nextPointPosition = CheckPointControllerList[checkPointNumber + 1].transform.position;
            float distanceBetweenCheckPoint = (nextPointPosition - currentPointPosition).sqrMagnitude;
            float distanceFromCheckPoint = (currentPointPosition - position).sqrMagnitude;

            prograss = Mathf.Clamp(distanceFromCheckPoint / distanceBetweenCheckPoint, 0.0f, 1.0f);
            return true;
        }
        else if (checkPointNumber >= CheckPointControllerList.Count)
        {
            prograss = 1;
        }

        return false;
    }
}
