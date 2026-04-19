using Consensus.Utils;
using Godot;

namespace Consensus.Models.Entities;

public abstract partial class GridEntity : Node2D
{
    public Vector2I GridPos { get; protected set; } = new();
    private TileMapLayer? _map;
    public TileMapLayer MapLayer => BasicUtil.Must(_map, GetType().Name);

    public virtual bool IsWalkable => true;

    public void Init(TileMapLayer map)
    {
        _map = map;
        GridPos = MapLayer.LocalToMap(Position);
        Position = MapLayer.MapToLocal(GridPos);
    }
}