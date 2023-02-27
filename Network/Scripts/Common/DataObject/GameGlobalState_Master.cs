using Network;

public class GameGlobalState_Master : MasterNetObject
{
    [Sirenix.OdinInspector.ShowInInspector]
    public GameGlobalState GameGlobalState { get; private set; } = new();

    public override void InitializeData(in MasterReplicationObject assignee)
    {
        GameGlobalState.InitializeDataAsMaster(assignee);
    }

}
