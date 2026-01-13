extends Control

## Game UI Controller
## Handles all UI screens: Main Menu, HUD, Game Over, Pause

@onready var main_menu: Control = $MainMenu
@onready var hud: Control = $HUD
@onready var game_over: Control = $GameOver
@onready var pause_menu: Control = $PauseMenu

# Main Menu elements
@onready var high_score_label: Label = $MainMenu/VBoxContainer/HighScoreLabel
@onready var classic_button: Button = $MainMenu/VBoxContainer/ClassicButton
@onready var arcade_button: Button = $MainMenu/VBoxContainer/ArcadeButton
@onready var zen_button: Button = $MainMenu/VBoxContainer/ZenButton

# HUD elements
@onready var score_label: Label = $HUD/ScoreLabel
@onready var lives_container: HBoxContainer = $HUD/LivesContainer
@onready var timer_label: Label = $HUD/TimerLabel
@onready var combo_label: Label = $HUD/ComboLabel
@onready var pause_button: Button = $HUD/PauseButton

# Game Over elements
@onready var final_score_label: Label = $GameOver/Panel/VBoxContainer/FinalScoreLabel
@onready var new_high_score_label: Label = $GameOver/Panel/VBoxContainer/NewHighScoreLabel
@onready var restart_button: Button = $GameOver/Panel/VBoxContainer/RestartButton
@onready var menu_button: Button = $GameOver/Panel/VBoxContainer/MenuButton
@onready var watch_ad_button: Button = $GameOver/Panel/VBoxContainer/WatchAdButton

# Pause Menu elements
@onready var resume_button: Button = $PauseMenu/Panel/VBoxContainer/ResumeButton
@onready var quit_button: Button = $PauseMenu/Panel/VBoxContainer/QuitButton

var life_icons: Array[TextureRect] = []
var combo_tween: Tween


func _ready() -> void:
	# Allow UI to work when game is paused
	process_mode = Node.PROCESS_MODE_ALWAYS

	# Connect game signals
	GameManager.score_changed.connect(_on_score_changed)
	GameManager.lives_changed.connect(_on_lives_changed)
	GameManager.combo_achieved.connect(_on_combo_achieved)
	GameManager.game_over.connect(_on_game_over)
	GameManager.game_started.connect(_on_game_started)
	GameManager.timer_updated.connect(_on_timer_updated)

	# Connect button signals
	classic_button.pressed.connect(_on_classic_pressed)
	arcade_button.pressed.connect(_on_arcade_pressed)
	zen_button.pressed.connect(_on_zen_pressed)
	pause_button.pressed.connect(_on_pause_pressed)
	restart_button.pressed.connect(_on_restart_pressed)
	menu_button.pressed.connect(_on_menu_pressed)
	watch_ad_button.pressed.connect(_on_watch_ad_pressed)
	resume_button.pressed.connect(_on_resume_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

	# Connect AdMob signals
	AdMobManager.rewarded_earned.connect(_on_rewarded_earned)

	# Initialize UI
	show_main_menu()
	update_high_score_display()


func show_main_menu() -> void:
	main_menu.visible = true
	hud.visible = false
	game_over.visible = false
	pause_menu.visible = false
	update_high_score_display()


func show_hud() -> void:
	main_menu.visible = false
	hud.visible = true
	game_over.visible = false
	pause_menu.visible = false


func show_game_over(score: int, is_new_high: bool) -> void:
	main_menu.visible = false
	hud.visible = true
	game_over.visible = true
	pause_menu.visible = false

	final_score_label.text = "Score: " + str(score)
	new_high_score_label.visible = is_new_high

	# Show watch ad button only if rewarded ad is ready
	watch_ad_button.visible = AdMobManager.is_rewarded_ready()


func show_pause_menu() -> void:
	pause_menu.visible = true


func hide_pause_menu() -> void:
	pause_menu.visible = false


func update_high_score_display() -> void:
	high_score_label.text = "High Score: " + str(GameManager.high_score)


func _on_score_changed(new_score: int) -> void:
	score_label.text = "Score: " + str(new_score)


func _on_lives_changed(new_lives: int) -> void:
	# Update lives display
	for i in range(lives_container.get_child_count()):
		var icon = lives_container.get_child(i)
		icon.visible = i < new_lives


func _on_combo_achieved(combo_count: int) -> void:
	combo_label.text = str(combo_count) + "x COMBO!"

	# Animate combo text
	if combo_tween:
		combo_tween.kill()

	combo_label.scale = Vector2.ONE
	combo_tween = create_tween()
	combo_tween.tween_property(combo_label, "scale", Vector2(1.5, 1.5), 0.1)
	combo_tween.tween_property(combo_label, "scale", Vector2.ONE, 0.1)
	combo_tween.tween_interval(1.0)
	combo_tween.tween_property(combo_label, "modulate:a", 0.0, 0.3)
	combo_tween.tween_callback(func():
		combo_label.text = ""
		combo_label.modulate.a = 1.0
	)


func _on_game_over(final_score: int, is_new_high: bool) -> void:
	AudioManager.play_sfx("game_over")
	if is_new_high:
		AudioManager.play_sfx("new_highscore")
	show_game_over(final_score, is_new_high)


func _on_game_started(mode: GameManager.GameMode) -> void:
	show_hud()

	# Setup timer visibility based on mode
	timer_label.visible = mode != GameManager.GameMode.CLASSIC

	# Reset displays
	score_label.text = "Score: 0"
	combo_label.text = ""

	# Setup lives display for Classic mode
	setup_lives_display(mode == GameManager.GameMode.CLASSIC)


func _on_timer_updated(time_left: float) -> void:
	timer_label.text = str(int(time_left))

	# Flash timer when low
	if time_left <= 10:
		timer_label.modulate = Color.RED if int(time_left) % 2 == 0 else Color.WHITE
	else:
		timer_label.modulate = Color.WHITE


func setup_lives_display(show_lives: bool) -> void:
	lives_container.visible = show_lives

	# Clear existing icons
	for child in lives_container.get_children():
		child.queue_free()
	life_icons.clear()

	if not show_lives:
		return

	# Create life icons
	for i in range(GameManager.max_lives):
		var icon = TextureRect.new()
		icon.custom_minimum_size = Vector2(30, 30)
		# You would load a heart texture here
		# icon.texture = preload("res://assets/sprites/heart.png")
		lives_container.add_child(icon)
		life_icons.append(icon)


#region Button Handlers

func _on_classic_pressed() -> void:
	AudioManager.play_sfx("button_click")
	GameManager.start_game(GameManager.GameMode.CLASSIC)


func _on_arcade_pressed() -> void:
	AudioManager.play_sfx("button_click")
	GameManager.start_game(GameManager.GameMode.ARCADE)


func _on_zen_pressed() -> void:
	AudioManager.play_sfx("button_click")
	GameManager.start_game(GameManager.GameMode.ZEN)


func _on_pause_pressed() -> void:
	AudioManager.play_sfx("button_click")
	get_tree().paused = true
	show_pause_menu()


func _on_restart_pressed() -> void:
	AudioManager.play_sfx("button_click")
	GameManager.restart_game()


func _on_menu_pressed() -> void:
	AudioManager.play_sfx("button_click")
	GameManager.return_to_menu()
	show_main_menu()


func _on_watch_ad_pressed() -> void:
	AudioManager.play_sfx("button_click")
	AdMobManager.show_rewarded()


func _on_resume_pressed() -> void:
	AudioManager.play_sfx("button_click")
	get_tree().paused = false
	hide_pause_menu()


func _on_quit_pressed() -> void:
	AudioManager.play_sfx("button_click")
	get_tree().paused = false
	GameManager.return_to_menu()
	show_main_menu()

#endregion


func _on_rewarded_earned(_type: String, _amount: int) -> void:
	# Grant extra life and continue game
	GameManager.lives += 1
	GameManager.lives_changed.emit(GameManager.lives)
	GameManager.is_playing = true
	game_over.visible = false
	get_tree().paused = false
