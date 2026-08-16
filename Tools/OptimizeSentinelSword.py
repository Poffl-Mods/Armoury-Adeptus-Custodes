import argparse
import json
import math
import os
import sys

import bpy


def triangle_count(mesh):
    mesh.calc_loop_triangles()
    return len(mesh.loop_triangles)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--report", required=True)
    parser.add_argument("--target-triangles", type=int, default=35000)
    script_args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(script_args)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.abspath(args.input), use_anim=False)

    mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not mesh_objects:
        raise RuntimeError("The FBX contains no mesh objects")

    original_triangles = sum(triangle_count(obj.data) for obj in mesh_objects)
    original_vertices = sum(len(obj.data.vertices) for obj in mesh_objects)
    ratio = min(1.0, args.target_triangles / max(1, original_triangles))

    for obj in mesh_objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(name="SentinelSword_GameReady", type="DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        modifier.use_symmetry = False
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
        obj.data.update()
        obj.select_set(False)

    optimized_triangles = sum(triangle_count(obj.data) for obj in mesh_objects)
    optimized_vertices = sum(len(obj.data.vertices) for obj in mesh_objects)

    # FBX export expects the objects to be selected when use_selection is enabled.
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        obj.select_set(True)

    output = os.path.abspath(args.output)
    os.makedirs(os.path.dirname(output), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=output,
        use_selection=True,
        apply_unit_scale=True,
        bake_space_transform=False,
        add_leaf_bones=False,
        use_armature_deform_only=True,
        mesh_smooth_type="OFF",
        path_mode="AUTO",
        embed_textures=False,
        bake_anim=False,
    )

    dimensions = []
    for obj in mesh_objects:
        dimensions.append([round(v, 6) for v in obj.dimensions])

    report = {
        "input": os.path.abspath(args.input),
        "output": output,
        "mesh_count": len(mesh_objects),
        "original_vertices": original_vertices,
        "original_triangles": original_triangles,
        "target_triangles": args.target_triangles,
        "decimate_ratio": ratio,
        "optimized_vertices": optimized_vertices,
        "optimized_triangles": optimized_triangles,
        "reduction_percent": round((1.0 - optimized_triangles / original_triangles) * 100.0, 2),
        "object_dimensions": dimensions,
    }
    with open(args.report, "w", encoding="utf-8") as handle:
        json.dump(report, handle, indent=2)
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
