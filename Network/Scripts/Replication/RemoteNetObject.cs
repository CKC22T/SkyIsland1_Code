using Network;
using UnityEngine;

public abstract class RemoteNetObject : MonoBehaviour
{
    private RemoteReplicationObject mDataObject;

    public virtual void BindViaRemoteManager(in RemoteReplicationObject newReplicationObject)
    {
        mDataObject = newReplicationObject;
        InitializeData(mDataObject);
    }

    public abstract void InitializeData(in RemoteReplicationObject assignee);

    public void ReadFromBuffer(ref NetBuffer buffer) => mDataObject.ReadFromBuffer(ref buffer);

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

    public virtual void OnDestroy()
    {
        Dispose();
    }

    #endregion
}