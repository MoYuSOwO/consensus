using Godot;

namespace Consensus.Models.Entities.Interactables;

public partial class Coin : GridInteractable
{
    public override void OnEnter()
    {
        EmitSignal(GridInteractable.SignalName.Activated);
    }
}