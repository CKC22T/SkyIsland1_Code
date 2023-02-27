using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterNetObject : MonoBehaviour
{
    // Replicable Object
    private MasterReplicationObject mDataObject; 

    public virtual void BindViaMasterManager(in MasterReplicationObject newReplicationObject)
    {
        mDataObject = newReplicationObject;
        InitializeData(mDataObject);
    }

    public abstract void InitializeData(in MasterReplicationObject assignee);

    #region Dispose

    // Dispose data object

    private bool mIsDisposed = false;

    public void Dispose()
    {
        if (mIsDisposed)
        {
            return;
        }

        mIsDisposed = true;

        mDataObject?.Dispose();
    }

    public void OnDestroy()
    {
        Dispose();
    }

    public void AppendAllUnreliableDataToStream(ref NetBuffer buffer) => mDataObject.AppendAllUnreliableDataToStream(ref buffer);

    public void AppendChangedReliableDataToStream(ref NetBuffer buffer) => mDataObject.AppendChangedReliableDataToStream(ref buffer);

    public void AppendChangedUnreliableDataToStream(ref NetBuffer buffer) => mDataObject.AppendChangedUnreliableDataToStream(ref buffer);

    public void AppendChangedAllDataToStream(ref NetBuffer buffer) => mDataObject.AppendChangedAllDataToStream(ref buffer);

    public void AppendAllDataToStream(ref NetBuffer buffer) => mDataObject.AppendAllDataToStream(ref buffer);

    #endregion
}