extends RefCounted
class_name CommandParam
	
var name: String
var data_type: int
var value: Variant

func _init(_name: String, _type: int, _value: Variant):
	name = _name
	data_type = _type
	value = _value
