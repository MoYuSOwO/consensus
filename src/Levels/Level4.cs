using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Models.Entities.Interactables;
using Consensus.Models.Entities.Recievers;
using Consensus.Utils;

namespace Consensus.Levels;

public partial class Level4 : LevelBase
{
    private bool IsFinish = false;

    private PressurePlate PressurePlateEntity => GetNode<PressurePlate>("Entities/PressurePlate");
    private Door DoorEntity => GetNode<Door>("Entities/Door");
    private Coin CoinEntity => GetNode<Coin>("Entities/Coin");

    protected override void InitLevel()
    {
        CoinEntity.Activated += Finish;
        PressurePlateEntity.Activated += DoorEntity.Open;
        PressurePlateEntity.Deactivated += DoorEntity.Close;
    }

    protected override bool IsGoalMet()
    {
        return IsFinish;
    }

    public void Finish()
    {
        IsFinish = true;
    }

}