extends Node

## Game Manager Singleton
## Handles game state, scoring, lives, and game modes

enum GameMode { CLASSIC, ARCADE, ZEN }

signal score_changed(new_score: int)
signal lives_changed(new_lives: int)
signal combo_achieved(combo_count: int)
signal game_over(final_score: int, is_new_highscore: bool)
signal game_started(mode: GameMode)
signal timer_updated(time_left: float)

# Game State
var current_mode: GameMode = GameMode.CLASSIC
var score: int = 0
var high_score: int = 0
var lives: int = 3
var is_playing: bool = false

# Combo System
var current_combo: int = 0
var last_slice_time: float = 0.0
@export var combo_time_window: float = 0.5
@export var combo_threshold: int = 3

# Timer (for Arcade/Zen modes)
var game_timer: float = 0.0
var timer_active: bool = false
@export var arcade_time: float = 60.0
@export var zen_time: float = 90.0

# Settings
@export var max_lives: int = 3

# References
var main_scene: Node = null


func _ready() -> void:
	high_score = load_high_score()
	process_mode = Node.PROCESS_MODE_ALWAYS


func _process(delta: float) -> void:
	if not is_playing:
		return

	# Update timer for Arcade/Zen modes
	if timer_active:
		game_timer -= delta
		timer_updated.emit(game_timer)

		if game_timer <= 0:
			game_timer = 0
			end_game()

	# Check combo timeout
	if current_combo > 0 and Time.get_ticks_msec() / 1000.0 - last_slice_time > combo_time_window:
		reset_combo()


#region Game State Management

func start_game(mode: GameMode) -> void:
	current_mode = mode
	is_playing = true
	score = 0
	lives = max_lives
	current_combo = 0

	# Setup based on game mode
	match mode:
		GameMode.CLASSIC:
			timer_active = false
		GameMode.ARCADE:
			game_timer = arcade_time
			timer_active = true
		GameMode.ZEN:
			game_timer = zen_time
			timer_active = true

	score_changed.emit(score)
	lives_changed.emit(lives)
	game_started.emit(mode)

	get_tree().paused = false


func end_game() -> void:
	is_playing = false
	get_tree().paused = true

	var is_new_high = score > high_score
	if is_new_high:
		high_score = score
		save_high_score()

	game_over.emit(score, is_new_high)


func restart_game() -> void:
	start_game(current_mode)


func return_to_menu() -> void:
	is_playing = false
	get_tree().paused = false
	# Signal to show menu will be handled by UI

#endregion


#region Scoring & Combo

func add_score(points: int) -> void:
	if not is_playing:
		return

	current_combo += 1
	last_slice_time = Time.get_ticks_msec() / 1000.0

	# Calculate combo bonus
	var combo_bonus: int = 0
	if current_combo >= combo_threshold:
		combo_bonus = current_combo - 1
		combo_achieved.emit(current_combo)

	var total_points = points + combo_bonus
	score += total_points
	score_changed.emit(score)

	# Update high score in real-time
	if score > high_score:
		high_score = score


func reset_combo() -> void:
	current_combo = 0


func on_fruit_missed() -> void:
	if current_mode != GameMode.CLASSIC:
		return
	if not is_playing:
		return

	lives -= 1
	lives_changed.emit(lives)

	if lives <= 0:
		end_game()


func on_bomb_hit() -> void:
	if not is_playing:
		return

	reset_combo()

	match current_mode:
		GameMode.CLASSIC:
			end_game()
		GameMode.ARCADE:
			# Lose 10 points in arcade mode
			score = max(0, score - 10)
			score_changed.emit(score)
		GameMode.ZEN:
			# No bombs in Zen mode, but just in case
			pass

#endregion


#region Save/Load

func save_high_score() -> void:
	var config = ConfigFile.new()
	config.set_value("game", "high_score", high_score)
	config.save("user://save_data.cfg")


func load_high_score() -> int:
	var config = ConfigFile.new()
	var err = config.load("user://save_data.cfg")
	if err == OK:
		return config.get_value("game", "high_score", 0)
	return 0

#endregion


#region Utility

func get_bomb_chance() -> float:
	match current_mode:
		GameMode.ZEN:
			return 0.0
		_:
			return 0.05

#endregion
