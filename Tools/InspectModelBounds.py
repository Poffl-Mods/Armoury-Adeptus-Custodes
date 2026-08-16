import os
import sys

import bpy
from mathutils import Vector


path = os.path.abspath(sys.argv[sys.argv.index("--") + 1])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=path, use_anim=False)

for obj in (item for item in bpy.context.scene.objects if item.type == "MESH"):
    corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    low = Vector(tuple(min(c[i] for c in corners) for i in range(3)))
    high = Vector(tuple(max(c[i] for c in corners) for i in range(3)))
    print(
        "SENTINEL_BOUNDS",
        obj.name,
        "min=", tuple(round(v, 6) for v in low),
        "max=", tuple(round(v, 6) for v in high),
        "center=", tuple(round(v, 6) for v in ((low + high) / 2)),
        "size=", tuple(round(v, 6) for v in (high - low)),
        "origin=", tuple(round(v, 6) for v in obj.matrix_world.translation),
    )
