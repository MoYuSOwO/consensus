using Consensus.Utils;
using Godot;

namespace Consensus.Models.Entities;

public abstract partial class GridInteractable : GridEntity
{
    [Signal] 
    public delegate void ActivatedEventHandler();

    [Signal] 
    public delegate void DeactivatedEventHandler();
    
    public override bool IsWalkable => true;

    public abstract void OnEnter();
    public virtual void OnExit() {}
}