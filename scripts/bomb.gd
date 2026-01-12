extends RigidBody2D
class_name Bomb

## Bomb
## Ends game or deducts points when sliced

signal bomb_hit()

@export var explosion_radius: float = 100.0
@export var missed_y_position: float = 800.0

var has_been_hit: bool = false
var initial_velocity: Vector2 = Vector2.ZERO

@onready var sprite: Sprite2D = $Sprite2D
@onready var collision: CollisionShape2D = $CollisionShape2D
@onready var slice_area: Area2D = $SliceArea
@onready var explosion_particles: GPUParticles2D = $ExplosionParticles
@onready var fuse_particles: GPUParticles2D = $FuseParticles


func _ready() -> void:
	if fuse_particles:
		fuse_particles.emitting = true

	# Apply initial velocity
	if initial_velocity != Vector2.ZERO:
		linear_velocity = initial_velocity


func _physics_process(_delta: float) -> void:
	if has_been_hit:
		return

	# Remove bomb if it falls below screen (no penalty)
	if global_position.y > missed_y_position:
		queue_free()


func set_initial_velocity(velocity: Vector2) -> void:
	initial_velocity = velocity
	if is_inside_tree():
		linear_velocity = velocity


func slice(_direction: Vector2) -> void:
	if has_been_hit:
		return

	has_been_hit = true

	# Play explosion
	AudioManager.play_sfx("explosion")

	# Show explosion effect
	if explosion_particles:
		explosion_particles.emitting = true

	# Hide sprite
	if sprite:
		sprite.visible = false

	# Notify game manager
	GameManager.on_bomb_hit()
	bomb_hit.emit()

	# Screen shake effect
	trigger_screen_shake()

	# Wait for particles then remove
	await get_tree().create_timer(1.0).timeout
	queue_free()


func trigger_screen_shake() -> void:
	var camera = get_viewport().get_camera_2d()
	if camera and camera.has_method("shake"):
		camera.shake(0.3, 10.0)
