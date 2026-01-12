@tool
extends EditorScript

## Run this script in Godot: File > Run
## Creates placeholder sprites for testing

func _run() -> void:
	var sprites = {
		"apple": Color.RED,
		"orange": Color.ORANGE,
		"watermelon": Color.GREEN,
		"lemon": Color.YELLOW,
		"kiwi": Color.SADDLE_BROWN,
		"bomb": Color.BLACK
	}

	for sprite_name in sprites:
		create_circle_sprite(sprite_name, sprites[sprite_name], 128)

	# Create heart for lives
	create_circle_sprite("heart", Color.RED, 32)

	print("Placeholder sprites created in res://assets/sprites/")


func create_circle_sprite(name: String, color: Color, size: int) -> void:
	var image = Image.create(size, size, false, Image.FORMAT_RGBA8)
	var center = Vector2(size / 2, size / 2)
	var radius = size / 2 - 2

	for x in range(size):
		for y in range(size):
			var pos = Vector2(x, y)
			if pos.distance_to(center) <= radius:
				image.set_pixel(x, y, color)
			else:
				image.set_pixel(x, y, Color.TRANSPARENT)

	var path = "res://assets/sprites/" + name + ".png"
	image.save_png(path)
