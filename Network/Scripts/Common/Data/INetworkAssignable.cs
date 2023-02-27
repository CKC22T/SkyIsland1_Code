using Network;

public interface INetworkAssignable
{
    public void InitializeDataAsMaster(in MasterReplicationObject assignee);
    public void InitializeDataAsRemote(in RemoteReplicationObject assignee);
    public void ResetData();
}