extends RigidBody2D
class_name Fruit

## Fruit
## Handles fruit physics and slicing behavior

signal fruit_sliced(points: int, position: Vector2)
signal fruit_missed()

@export var base_points: int = 1
@export var sprite_texture: Texture2D
@export var slice_left_texture: Texture2D
@export var slice_right_texture: Texture2D
@export var juice_color: Color = Color.RED
@export var missed_y_position: float = 800.0

var has_been_sliced: bool = false
var has_missed: bool = false
var initial_velocity: Vector2 = Vector2.ZERO

@onready var sprite: Sprite2D = $Sprite2D
@onready var collision: CollisionShape2D = $CollisionShape2D
@onready var slice_area: Area2D = $SliceArea
@onready var particles: GPUParticles2D = $JuiceParticles


func _ready() -> void:
	if sprite:
		if sprite_texture:
			sprite.texture = sprite_texture
		else:
			# Create a default colored circle texture if none assigned
			_create_default_texture()

	if particles:
		particles.modulate = juice_color
		particles.emitting = false

	# Apply initial velocity
	if initial_velocity != Vector2.ZERO:
		linear_velocity = initial_velocity


func _create_default_texture() -> void:
	# Create a simple colored circle as placeholder
	var img = Image.create(80, 80, false, Image.FORMAT_RGBA8)
	var center = Vector2(40, 40)
	var radius = 38.0

	for x in range(80):
		for y in range(80):
			var dist = Vector2(x, y).distance_to(center)
			if dist <= radius:
				img.set_pixel(x, y, juice_color)
			else:
				img.set_pixel(x, y, Color.TRANSPARENT)

	var tex = ImageTexture.create_from_image(img)
	sprite.texture = tex


func _physics_process(_delta: float) -> void:
	if has_been_sliced or has_missed:
		return

	# Check if fruit missed (fell below screen)
	if global_position.y > missed_y_position:
		on_missed()


func set_initial_velocity(velocity: Vector2) -> void:
	initial_velocity = velocity
	if is_inside_tree():
		linear_velocity = velocity


func slice(direction: Vector2) -> void:
	if has_been_sliced:
		return

	has_been_sliced = true

	# Emit particles
	if particles:
		particles.emitting = true

	# Play slice sound
	AudioManager.play_sfx("slice")

	# Add score
	GameManager.add_score(base_points)
	fruit_sliced.emit(base_points, global_position)

	# Create sliced halves
	create_sliced_halves(direction)

	# Remove original fruit
	queue_free()


func create_sliced_halves(direction: Vector2) -> void:
	var slice_scene = preload("res://scenes/fruit_half.tscn")

	# Left half
	var left_half = slice_scene.instantiate()
	left_half.global_position = global_position
	left_half.setup(slice_left_texture if slice_left_texture else sprite_texture, -direction, juice_color)
	get_parent().add_child(left_half)

	# Right half
	var right_half = slice_scene.instantiate()
	right_half.global_position = global_position
	right_half.setup(slice_right_texture if slice_right_texture else sprite_texture, direction, juice_color)
	get_parent().add_child(right_half)


func on_missed() -> void:
	if has_missed or has_been_sliced:
		return

	has_missed = true
	fruit_missed.emit()
	GameManager.on_fruit_missed()

	# Fade out and remove
	var tween = create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.3)
	tween.tween_callback(queue_free)
