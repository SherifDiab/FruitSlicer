extends Node

## Audio Manager Singleton
## Handles all game audio: music and sound effects

@export var music_volume: float = 0.8
@export var sfx_volume: float = 1.0

var music_player: AudioStreamPlayer
var sfx_players: Array[AudioStreamPlayer] = []
var sfx_pool_size: int = 8

# Sound effect library
var sfx_library: Dictionary = {}


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

	# Create music player
	music_player = AudioStreamPlayer.new()
	music_player.bus = "Music"
	add_child(music_player)

	# Create SFX player pool
	for i in range(sfx_pool_size):
		var player = AudioStreamPlayer.new()
		player.bus = "SFX"
		add_child(player)
		sfx_players.append(player)

	# Load sound effects
	_load_sfx_library()


func _load_sfx_library() -> void:
	# Define paths to sound effects
	var sfx_paths = {
		"slice": "res://assets/audio/sfx/slice.wav",
		"slice_combo": "res://assets/audio/sfx/slice_combo.wav",
		"explosion": "res://assets/audio/sfx/explosion.wav",
		"whoosh": "res://assets/audio/sfx/whoosh.wav",
		"game_over": "res://assets/audio/sfx/game_over.wav",
		"button_click": "res://assets/audio/sfx/button_click.wav",
		"countdown": "res://assets/audio/sfx/countdown.wav",
		"new_highscore": "res://assets/audio/sfx/new_highscore.wav"
	}

	for key in sfx_paths:
		var path = sfx_paths[key]
		if ResourceLoader.exists(path):
			sfx_library[key] = load(path)
		else:
			push_warning("SFX not found: " + path)


func play_music(stream: AudioStream, fade_in: float = 0.5) -> void:
	if music_player.playing:
		# Crossfade
		var tween = create_tween()
		tween.tween_property(music_player, "volume_db", -80, fade_in)
		tween.tween_callback(func():
			music_player.stream = stream
			music_player.volume_db = linear_to_db(music_volume)
			music_player.play()
		)
	else:
		music_player.stream = stream
		music_player.volume_db = linear_to_db(music_volume)
		music_player.play()


func stop_music(fade_out: float = 0.5) -> void:
	if music_player.playing:
		var tween = create_tween()
		tween.tween_property(music_player, "volume_db", -80, fade_out)
		tween.tween_callback(music_player.stop)


func play_sfx(sfx_name: String, pitch_variance: float = 0.1) -> void:
	if not sfx_library.has(sfx_name):
		push_warning("SFX not in library: " + sfx_name)
		return

	var player = _get_available_sfx_player()
	if player:
		player.stream = sfx_library[sfx_name]
		player.volume_db = linear_to_db(sfx_volume)
		player.pitch_scale = 1.0 + randf_range(-pitch_variance, pitch_variance)
		player.play()


func play_sfx_stream(stream: AudioStream, pitch_variance: float = 0.1) -> void:
	var player = _get_available_sfx_player()
	if player:
		player.stream = stream
		player.volume_db = linear_to_db(sfx_volume)
		player.pitch_scale = 1.0 + randf_range(-pitch_variance, pitch_variance)
		player.play()


func _get_available_sfx_player() -> AudioStreamPlayer:
	for player in sfx_players:
		if not player.playing:
			return player

	# All players busy, return first one (will interrupt)
	return sfx_players[0]


func set_music_volume(volume: float) -> void:
	music_volume = clamp(volume, 0.0, 1.0)
	music_player.volume_db = linear_to_db(music_volume)


func set_sfx_volume(volume: float) -> void:
	sfx_volume = clamp(volume, 0.0, 1.0)
