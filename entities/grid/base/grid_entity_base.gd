extends Node2D
class_name GridEntityBase

var grid_pos: Vector2i = Vector2i.ZERO
var map_layer: TileMapLayer

func is_walkable() -> bool:
	return true

func init(p_map: TileMapLayer) -> void:
	map_layer = p_map
	grid_pos = map_layer.local_to_map(position)
	position = map_layer.map_to_local(grid_pos)
