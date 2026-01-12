extends Node2D

## Spawner
## Handles spawning of fruits and bombs

@export var fruit_scenes: Array[PackedScene] = []
@export var bomb_scene: PackedScene

@export_group("Spawn Settings")
@export var min_spawn_delay: float = 0.5
@export var max_spawn_delay: float = 1.5
@export var spawn_y_offset: float = 50.0

@export_group("Launch Settings")
@export var min_launch_force: float = 600.0
@export var max_launch_force: float = 900.0
@export var min_angle: float = 60.0  # degrees
@export var max_angle: float = 120.0  # degrees
@export var side_variance: float = 100.0

@export_group("Difficulty")
@export var initial_spawn_rate: float = 1.0
@export var spawn_rate_increase: float = 0.1
@export var max_spawn_rate: float = 3.0

var spawn_timer: Timer
var spawn_rate_multiplier: float = 1.0
var spawn_points: Array[Vector2] = []
var is_spawning: bool = false


func _ready() -> void:
	spawn_timer = Timer.new()
	spawn_timer.one_shot = true
	spawn_timer.timeout.connect(_on_spawn_timer_timeout)
	add_child(spawn_timer)

	# Calculate spawn points along the bottom of the screen
	_setup_spawn_points()

	# Connect to game signals
	GameManager.game_started.connect(_on_game_started)
	GameManager.game_over.connect(_on_game_over)


func _setup_spawn_points() -> void:
	var viewport_size = get_viewport_rect().size
	var num_points = 5

	spawn_points.clear()
	for i in range(num_points):
		var x = (viewport_size.x / (num_points + 1)) * (i + 1)
		var y = viewport_size.y + spawn_y_offset
		spawn_points.append(Vector2(x, y))


func _on_game_started(_mode: GameManager.GameMode) -> void:
	spawn_rate_multiplier = initial_spawn_rate
	is_spawning = true
	_schedule_next_spawn()


func _on_game_over(_score: int, _is_high: bool) -> void:
	is_spawning = false
	spawn_timer.stop()


func _schedule_next_spawn() -> void:
	if not is_spawning:
		return

	var delay = randf_range(min_spawn_delay, max_spawn_delay) / spawn_rate_multiplier
	spawn_timer.start(delay)


func _on_spawn_timer_timeout() -> void:
	if not is_spawning or not GameManager.is_playing:
		return

	spawn_wave()
	_increase_difficulty()
	_schedule_next_spawn()


func spawn_wave() -> void:
	# Determine wave size (1-3 fruits)
	var wave_size = randi_range(1, 3)

	# Maybe add a bomb
	var should_spawn_bomb = randf() < GameManager.get_bomb_chance()

	# Pick random spawn points for this wave
	var available_points = spawn_points.duplicate()
	available_points.shuffle()

	for i in range(wave_size):
		if i >= available_points.size():
			break

		var spawn_pos = available_points[i]
		spawn_pos.x += randf_range(-side_variance, side_variance)

		# Spawn bomb or fruit
		if should_spawn_bomb and i == 0:
			spawn_bomb(spawn_pos)
		else:
			spawn_fruit(spawn_pos)


func spawn_fruit(pos: Vector2) -> void:
	if fruit_scenes.is_empty():
		push_warning("No fruit scenes assigned to spawner!")
		return

	var fruit_scene = fruit_scenes[randi() % fruit_scenes.size()]
	var fruit = fruit_scene.instantiate() as Fruit

	fruit.global_position = pos
	fruit.set_initial_velocity(_calculate_launch_velocity(pos))

	get_parent().add_child(fruit)


func spawn_bomb(pos: Vector2) -> void:
	if not bomb_scene:
		push_warning("No bomb scene assigned to spawner!")
		return

	var bomb = bomb_scene.instantiate() as Bomb

	bomb.global_position = pos
	bomb.set_initial_velocity(_calculate_launch_velocity(pos))

	get_parent().add_child(bomb)


func _calculate_launch_velocity(spawn_pos: Vector2) -> Vector2:
	var viewport_center_x = get_viewport_rect().size.x / 2.0

	# Calculate angle based on position (outer positions aim more inward)
	var offset_from_center = spawn_pos.x - viewport_center_x
	var base_angle = 90.0  # Straight up

	# Adjust angle to aim toward center
	if abs(offset_from_center) > 50:
		base_angle += (offset_from_center / viewport_center_x) * -20.0

	# Add some randomness
	var angle = deg_to_rad(randf_range(base_angle - 15, base_angle + 15))

	# Calculate force
	var force = randf_range(min_launch_force, max_launch_force)

	# Create velocity vector (pointing up and slightly toward center)
	var velocity = Vector2(cos(angle - PI/2), -sin(angle - PI/2)) * force

	return velocity


func _increase_difficulty() -> void:
	spawn_rate_multiplier = min(spawn_rate_multiplier + spawn_rate_increase * 0.01, max_spawn_rate)
