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

	# Auto-load scenes if not assigned in editor
	_load_default_scenes()

	# Connect to game signals
	GameManager.game_started.connect(_on_game_started)
	GameManager.game_over.connect(_on_game_over)


func _load_default_scenes() -> void:
	# Load fruit scene if not assigned
	if fruit_scenes.is_empty():
		var fruit_path = "res://scenes/fruit.tscn"
		if ResourceLoader.exists(fruit_path):
			var fruit_scene = load(fruit_path)
			fruit_scenes.append(fruit_scene)
		else:
			print("ERROR: Could not find fruit scene at: ", fruit_path)

	# Load bomb scene if not assigned
	if not bomb_scene:
		var bomb_path = "res://scenes/bomb.tscn"
		if ResourceLoader.exists(bomb_path):
			bomb_scene = load(bomb_path)


func _setup_spawn_points() -> void:
	var viewport_size = get_viewport_rect().size
	print("Viewport size: ", viewport_size)

	# Fallback if viewport not ready
	if viewport_size.x <= 0 or viewport_size.y <= 0:
		viewport_size = Vector2(1080, 1920)  # Default mobile portrait
		print("Using fallback viewport size")

	var num_points = 5

	spawn_points.clear()
	for i in range(num_points):
		var x = (viewport_size.x / (num_points + 1)) * (i + 1)
		var y = viewport_size.y + spawn_y_offset
		spawn_points.append(Vector2(x, y))

	print("Spawn points created: ", spawn_points)


func _on_game_started(_mode: GameManager.GameMode) -> void:
	# Setup spawn points now that viewport is ready
	_setup_spawn_points()

	spawn_rate_multiplier = initial_spawn_rate
	is_spawning = true

	print("Game started! Spawning enabled. Spawn points: ", spawn_points.size())
	print("Fruit scenes loaded: ", fruit_scenes.size())

	# Start spawning immediately
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
	print("Spawning wave...")

	# Determine wave size (1-3 fruits)
	var wave_size = randi_range(1, 3)

	# Maybe add a bomb
	var should_spawn_bomb = randf() < GameManager.get_bomb_chance()

	# Pick random spawn points for this wave
	var available_points = spawn_points.duplicate()
	available_points.shuffle()

	print("Wave size: ", wave_size, " Available points: ", available_points.size())

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
		print("ERROR: No fruit scenes!")
		return

	var fruit_scene = fruit_scenes[randi() % fruit_scenes.size()]
	var fruit = fruit_scene.instantiate()

	# Assign random fruit color for variety
	var fruit_colors = [
		Color.RED,        # Apple
		Color.ORANGE,     # Orange
		Color.YELLOW,     # Lemon
		Color.GREEN,      # Watermelon
		Color.PURPLE,     # Grape
		Color(0.6, 0.4, 0.2)  # Kiwi
	]
	fruit.juice_color = fruit_colors[randi() % fruit_colors.size()]

	fruit.global_position = pos
	var velocity = _calculate_launch_velocity(pos)
	fruit.set_initial_velocity(velocity)

	get_tree().current_scene.add_child(fruit)
	print("Fruit spawned at: ", pos, " velocity: ", velocity)


func spawn_bomb(pos: Vector2) -> void:
	if not bomb_scene:
		print("ERROR: No bomb scene!")
		return

	var bomb = bomb_scene.instantiate()

	bomb.global_position = pos
	var velocity = _calculate_launch_velocity(pos)
	bomb.set_initial_velocity(velocity)

	get_tree().current_scene.add_child(bomb)
	print("Bomb spawned at: ", pos, " velocity: ", velocity)


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
