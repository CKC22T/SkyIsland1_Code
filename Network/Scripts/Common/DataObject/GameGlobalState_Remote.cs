using Network;

public class GameGlobalState_Remote : RemoteNetObject
{
    [Sirenix.OdinInspector.ShowInInspector]
    public GameGlobalState GameGlobalState { get; private set;} = new();

    public override void InitializeData(in RemoteReplicationObject assignee)
    {
        GameGlobalState.InitializeDataAsRemote(assignee);
    }
}
