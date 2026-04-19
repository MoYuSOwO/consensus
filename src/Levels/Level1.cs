using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Models.Entities.Interactables;
using Consensus.Models.Entities.Recievers;
using Consensus.Utils;

namespace Consensus.Levels;

public partial class Level1 : LevelBase
{
    private bool IsFinish = false;

    private Coin CoinEntity => GetNode<Coin>("Entities/Coin");

    protected override void InitLevel()
    {
        CoinEntity.Activated += Finish;
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