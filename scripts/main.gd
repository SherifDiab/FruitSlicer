extends Node2D

## Main Scene Controller
## Handles scene management and game flow

@onready var game_area: Node2D = $GameArea
@onready var blade: Node2D = $GameArea/Blade
@onready var spawner: Node2D = $GameArea/Spawner
@onready var camera: Camera2D = $Camera2D


func _ready() -> void:
	GameManager.main_scene = self

	# Connect to game signals
	GameManager.game_started.connect(_on_game_started)
	GameManager.game_over.connect(_on_game_over)

	# Initialize AdMob
	if OS.get_name() == "Android":
		AdMobManager.preload_all_ads()
		AdMobManager.load_banner()


func _on_game_started(_mode: GameManager.GameMode) -> void:
	# Clear any existing fruits/bombs
	clear_game_objects()

	# Hide banner during gameplay
	AdMobManager.hide_banner()


func _on_game_over(_score: int, _is_high: bool) -> void:
	# Show interstitial ad occasionally
	if randi() % 3 == 0:  # 33% chance
		AdMobManager.show_interstitial()

	# Show banner
	AdMobManager.show_banner()


func clear_game_objects() -> void:
	for child in game_area.get_children():
		if child is Fruit or child is Bomb:
			child.queue_free()


func shake_camera(duration: float, intensity: float) -> void:
	if not camera:
		return

	var original_offset = camera.offset
	var shake_timer = 0.0

	while shake_timer < duration:
		var offset_x = randf_range(-intensity, intensity)
		var offset_y = randf_range(-intensity, intensity)
		camera.offset = original_offset + Vector2(offset_x, offset_y)

		await get_tree().process_frame
		shake_timer += get_process_delta_time()

	camera.offset = original_offset
