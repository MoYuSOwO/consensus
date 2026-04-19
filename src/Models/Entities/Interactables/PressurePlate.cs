using Godot;

namespace Consensus.Models.Entities.Interactables;

public partial class PressurePlate : GridInteractable
{
    private int robotsOnPlate = 0;

    public override void OnEnter()
    {
        robotsOnPlate++;
        if (robotsOnPlate == 1)
        {
            EmitSignal(GridInteractable.SignalName.Activated);
            Modulate = Colors.LimeGreen; 
        }
    }

    public override void OnExit()
    {
        robotsOnPlate--;
        if (robotsOnPlate <= 0)
        {
            robotsOnPlate = 0;
            EmitSignal(GridInteractable.SignalName.Deactivated);
            Modulate = Colors.White;
        }
    }
}