extends Node

## AdMob Manager Singleton
## Handles AdMob ads for Android using Poing Studios AdMob plugin
## Plugin: https://github.com/poing-studios/godot-admob-plugin

signal banner_loaded
signal banner_failed(error_code: int)
signal interstitial_loaded
signal interstitial_failed(error_code: int)
signal interstitial_closed
signal rewarded_loaded
signal rewarded_failed(error_code: int)
signal rewarded_earned(type: String, amount: int)
signal rewarded_closed

# Ad Unit IDs - Replace with your actual IDs
const BANNER_ID_ANDROID: String = "ca-app-pub-3940256099942544/6300978111"  # Test ID
const INTERSTITIAL_ID_ANDROID: String = "ca-app-pub-3940256099942544/1033173712"  # Test ID
const REWARDED_ID_ANDROID: String = "ca-app-pub-3940256099942544/5224354917"  # Test ID

# Production IDs (replace these with your actual ad unit IDs)
const PROD_BANNER_ID: String = "ca-app-pub-XXXXX/XXXXX"
const PROD_INTERSTITIAL_ID: String = "ca-app-pub-XXXXX/XXXXX"
const PROD_REWARDED_ID: String = "ca-app-pub-XXXXX/XXXXX"

@export var use_test_ads: bool = true
@export var is_real_device: bool = false

var admob: Object = null
var is_initialized: bool = false
var banner_loaded_flag: bool = false
var interstitial_loaded_flag: bool = false
var rewarded_loaded_flag: bool = false


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

	if OS.get_name() == "Android":
		_initialize_admob()


func _initialize_admob() -> void:
	if Engine.has_singleton("AdMob"):
		admob = Engine.get_singleton("AdMob")

		# Initialize with configuration
		var config = {
			"is_for_child_directed_treatment": false,
			"is_personalized": true,
			"is_real": is_real_device and not use_test_ads,
			"max_ad_content_rating": "G"
		}

		admob.initialize(config)
		is_initialized = true

		# Connect signals
		_connect_signals()

		print("AdMob initialized successfully")
	else:
		push_warning("AdMob singleton not found. Make sure the plugin is installed.")


func _connect_signals() -> void:
	if not admob:
		return

	# Banner signals
	if admob.has_signal("banner_loaded"):
		admob.banner_loaded.connect(_on_banner_loaded)
	if admob.has_signal("banner_failed_to_load"):
		admob.banner_failed_to_load.connect(_on_banner_failed)

	# Interstitial signals
	if admob.has_signal("interstitial_loaded"):
		admob.interstitial_loaded.connect(_on_interstitial_loaded)
	if admob.has_signal("interstitial_failed_to_load"):
		admob.interstitial_failed_to_load.connect(_on_interstitial_failed)
	if admob.has_signal("interstitial_closed"):
		admob.interstitial_closed.connect(_on_interstitial_closed)

	# Rewarded signals
	if admob.has_signal("rewarded_ad_loaded"):
		admob.rewarded_ad_loaded.connect(_on_rewarded_loaded)
	if admob.has_signal("rewarded_ad_failed_to_load"):
		admob.rewarded_ad_failed_to_load.connect(_on_rewarded_failed)
	if admob.has_signal("rewarded_ad_closed"):
		admob.rewarded_ad_closed.connect(_on_rewarded_closed)
	if admob.has_signal("user_earned_reward"):
		admob.user_earned_reward.connect(_on_user_earned_reward)


#region Banner Ads

func load_banner(position: String = "BOTTOM") -> void:
	if not is_initialized or not admob:
		push_warning("AdMob not initialized")
		return

	var ad_id = BANNER_ID_ANDROID if use_test_ads else PROD_BANNER_ID
	admob.load_banner(ad_id, position)


func show_banner() -> void:
	if not is_initialized or not admob:
		return

	if banner_loaded_flag:
		admob.show_banner()


func hide_banner() -> void:
	if not is_initialized or not admob:
		return

	admob.hide_banner()


func destroy_banner() -> void:
	if not is_initialized or not admob:
		return

	admob.destroy_banner()
	banner_loaded_flag = false


func _on_banner_loaded() -> void:
	banner_loaded_flag = true
	banner_loaded.emit()
	print("Banner ad loaded")


func _on_banner_failed(error_code: int) -> void:
	banner_loaded_flag = false
	banner_failed.emit(error_code)
	push_warning("Banner ad failed to load: " + str(error_code))

#endregion


#region Interstitial Ads

func load_interstitial() -> void:
	if not is_initialized or not admob:
		push_warning("AdMob not initialized")
		return

	var ad_id = INTERSTITIAL_ID_ANDROID if use_test_ads else PROD_INTERSTITIAL_ID
	admob.load_interstitial(ad_id)


func show_interstitial() -> void:
	if not is_initialized or not admob:
		return

	if interstitial_loaded_flag:
		admob.show_interstitial()
	else:
		push_warning("Interstitial not loaded yet")
		load_interstitial()


func _on_interstitial_loaded() -> void:
	interstitial_loaded_flag = true
	interstitial_loaded.emit()
	print("Interstitial ad loaded")


func _on_interstitial_failed(error_code: int) -> void:
	interstitial_loaded_flag = false
	interstitial_failed.emit(error_code)
	push_warning("Interstitial ad failed to load: " + str(error_code))


func _on_interstitial_closed() -> void:
	interstitial_loaded_flag = false
	interstitial_closed.emit()
	# Preload next interstitial
	load_interstitial()

#endregion


#region Rewarded Ads

func load_rewarded() -> void:
	if not is_initialized or not admob:
		push_warning("AdMob not initialized")
		return

	var ad_id = REWARDED_ID_ANDROID if use_test_ads else PROD_REWARDED_ID
	admob.load_rewarded_ad(ad_id)


func show_rewarded() -> void:
	if not is_initialized or not admob:
		return

	if rewarded_loaded_flag:
		admob.show_rewarded_ad()
	else:
		push_warning("Rewarded ad not loaded yet")
		load_rewarded()


func is_rewarded_ready() -> bool:
	return rewarded_loaded_flag


func _on_rewarded_loaded() -> void:
	rewarded_loaded_flag = true
	rewarded_loaded.emit()
	print("Rewarded ad loaded")


func _on_rewarded_failed(error_code: int) -> void:
	rewarded_loaded_flag = false
	rewarded_failed.emit(error_code)
	push_warning("Rewarded ad failed to load: " + str(error_code))


func _on_rewarded_closed() -> void:
	rewarded_loaded_flag = false
	rewarded_closed.emit()
	# Preload next rewarded ad
	load_rewarded()


func _on_user_earned_reward(type: String, amount: int) -> void:
	rewarded_earned.emit(type, amount)
	print("User earned reward: " + type + " x" + str(amount))

#endregion


#region Utility

func preload_all_ads() -> void:
	load_interstitial()
	load_rewarded()


func set_test_mode(enabled: bool) -> void:
	use_test_ads = enabled

#endregion
