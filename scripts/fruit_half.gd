extends RigidBody2D

## FruitHalf
## Sliced fruit half that falls away

@export var fade_time: float = 2.0
@export var spin_speed: float = 5.0

@onready var sprite: Sprite2D = $Sprite2D
@onready var particles: GPUParticles2D = $JuiceParticles


func _ready() -> void:
	# Start fade out
	var tween = create_tween()
	tween.tween_property(self, "modulate:a", 0.0, fade_time)
	tween.tween_callback(queue_free)


func setup(texture: Texture2D, direction: Vector2, color: Color) -> void:
	if sprite:
		sprite.texture = texture

	if particles:
		particles.modulate = color
		particles.emitting = true

	# Apply velocity in slice direction
	var force = direction * randf_range(200, 400)
	force.y = -randf_range(100, 300)  # Add upward component
	linear_velocity = force

	# Add spin
	angular_velocity = spin_speed * (1 if direction.x > 0 else -1)
