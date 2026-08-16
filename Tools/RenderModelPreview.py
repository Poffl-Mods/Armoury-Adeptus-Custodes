import argparse
import math
import os
import sys

import bpy
from mathutils import Vector


def point_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    script_args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(script_args)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.abspath(args.input), use_anim=False)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No meshes found")

    corners = []
    for obj in meshes:
        corners.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    center = sum(corners, Vector()) / len(corners)
    extent = max((corner - center).length for corner in corners)

    camera_data = bpy.data.cameras.new("PreviewCamera")
    camera = bpy.data.objects.new("PreviewCamera", camera_data)
    bpy.context.scene.collection.objects.link(camera)
    camera.location = center + Vector((0.0, -extent * 3.0, 0.0))
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = extent * 2.25
    point_at(camera, center)
    bpy.context.scene.camera = camera

    for name, location, energy, size in (
        ("Key", center + Vector((-2.5, -3.0, 3.0)), 1100, 3.0),
        ("Fill", center + Vector((2.5, -2.0, 1.0)), 800, 2.5),
        ("Rim", center + Vector((0.0, 2.0, 2.0)), 1000, 2.0),
    ):
        data = bpy.data.lights.new(name, "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        light = bpy.data.objects.new(name, data)
        bpy.context.scene.collection.objects.link(light)
        light.location = location
        point_at(light, center)

    world = bpy.context.scene.world
    if world is None:
        world = bpy.data.worlds.new("PreviewWorld")
        bpy.context.scene.world = world
    world.color = (0.008, 0.008, 0.012)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "SINGLE"
    scene.display.shading.single_color = (0.32, 0.34, 0.38)
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.shading.cavity_type = "WORLD"
    scene.render.resolution_x = 700
    scene.render.resolution_y = 1000
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = os.path.abspath(args.output)
    scene.render.film_transparent = False
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    main()
