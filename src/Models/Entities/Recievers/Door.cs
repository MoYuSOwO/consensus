using Godot;

namespace Consensus.Models.Entities.Recievers;

public partial class Door : GridEntity
{
    private bool isOpen = false;

    public override bool IsWalkable => isOpen;

    private Sprite2D Body => GetNode<Sprite2D>("Body");

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        
        Body.Frame = 1;
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        
        Body.Frame = 0;
    }
}