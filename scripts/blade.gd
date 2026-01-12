extends Node2D

## Blade/Slicer
## Handles player input for slicing fruits

signal sliced(position: Vector2, direction: Vector2)

@export var trail_length: int = 20
@export var min_slice_velocity: float = 100.0

var is_slicing: bool = false
var slice_positions: Array[Vector2] = []
var previous_position: Vector2 = Vector2.ZERO

@onready var trail_line: Line2D = $TrailLine
@onready var slice_area: Area2D = $SliceArea


func _ready() -> void:
	if trail_line:
		trail_line.clear_points()
	slice_positions.clear()


func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				start_slice(event.position)
			else:
				end_slice()

	elif event is InputEventMouseMotion and is_slicing:
		update_slice(event.position)

	# Touch support for mobile
	elif event is InputEventScreenTouch:
		if event.pressed:
			start_slice(event.position)
		else:
			end_slice()

	elif event is InputEventScreenDrag and is_slicing:
		update_slice(event.position)


func start_slice(pos: Vector2) -> void:
	is_slicing = true
	slice_positions.clear()
	slice_positions.append(pos)
	previous_position = pos

	if trail_line:
		trail_line.clear_points()
		trail_line.add_point(pos)


func update_slice(pos: Vector2) -> void:
	if not is_slicing:
		return

	slice_positions.append(pos)

	# Keep trail at max length
	while slice_positions.size() > trail_length:
		slice_positions.remove_at(0)

	# Update trail visual
	if trail_line:
		trail_line.add_point(pos)
		while trail_line.get_point_count() > trail_length:
			trail_line.remove_point(0)

	# Calculate velocity
	var velocity = pos - previous_position
	var speed = velocity.length()

	# Check for slicing
	if speed >= min_slice_velocity:
		check_slice(pos, velocity.normalized())

	previous_position = pos


func end_slice() -> void:
	is_slicing = false
	slice_positions.clear()

	if trail_line:
		# Fade out trail
		var tween = create_tween()
		tween.tween_property(trail_line, "modulate:a", 0.0, 0.2)
		tween.tween_callback(func():
			trail_line.clear_points()
			trail_line.modulate.a = 1.0
		)


func check_slice(pos: Vector2, direction: Vector2) -> void:
	# Move slice area to current position
	if slice_area:
		slice_area.global_position = pos

		# Check for overlapping bodies
		var overlapping = slice_area.get_overlapping_areas()
		for area in overlapping:
			var parent = area.get_parent()
			if parent.has_method("slice"):
				parent.slice(direction)
				sliced.emit(pos, direction)
