# First custom Guardian Spear asset

## Version 0.1.16: rifle-forward axis correction

The previous correction rotated around local X, which tilted the spear but left its longitudinal
axis mostly sideways. The firing pose now applies the quarter-turn around local Y instead, mapping
the horizontal spear onto the rifle animation's forward Z axis. The successful pre-shot timing from
version 0.1.15 remains unchanged.

## Version 0.1.15: target-axis correction and immediate firing pose

The firing pose now adds the missing 90-degree local-axis correction so the spear tip follows the
rifle animation's target direction instead of lying across it. At execution start the visual root
and its muzzle locator snap directly into the firing rotation; this removes the previous delay where
the projectile could be emitted before the transition from vertical to horizontal had completed.
The existing eased return to the upright melee pose remains unchanged.

## Version 0.1.14: global execution event and visible hold

The 0.1.13 live test showed no pose change. Reflection confirms that
`IAbilityExecutionProcessHandler` is caster-scoped through `ISubscriber<IMechanicEntity>`, while an
equipment prefab has no subscription entity. The controller now uses Owlcat's global subscription
and filters the unique Guardian Spear Bolt Shot GUID. Rotation starts immediately at execution start,
takes about 0.17 seconds at 540 degrees/second, remains horizontal throughout execution and is held
for another 0.45 seconds after execution end before returning vertically. The owner filter and initial
0.15-second delay are removed. Single-shot mechanics remain unchanged; no Harmony is used.

## Version 0.1.13: execution-time visual shot pose

Bolt Shot is restored to the regular `SingleShot` slot and delivery; the temporary `Special: Burst`
and `RateOfFire: 1` experiment are removed. `GuardianSpearShotPoseController` subscribes directly to
Owlcat's `IAbilityExecutionProcessHandler`, filters the Guardian Spear Bolt Shot GUID and owning unit,
then rotates only `GuardianSpear_Visual` from idle Z `+45` to shot Z `-45` after a 0.15-second delay.
It rotates at 720 degrees/second and restores the idle pose when execution ends. The muzzle locator is
now a child of that pose transform at local `(0.4384062, 0.523259, 0.08)`, equivalent to its previous
idle root position, so projectile and muzzle FX follow the temporary horizontal pose. This uses no
Harmony and introduces no external mod dependency.

## Version 0.1.12: single-projectile burst-animation test

The 0.1.11 live test proved that the hidden `Rifle` style is consumed, but the ordinary single-shot
pose does not aim the long visible spear. Rogue Trader's standard single and burst abilities both use
`Animation: Directional`; their relevant difference is `WarhammerAbilityAttackDelivery.Special`.
Bolt Shot now requests `Special: Burst` and its visible weapon slot is marked `Burst`, while the hidden
profile is forced to `RateOfFire: 1`. It therefore tests the longer braced burst pose with one intended
projectile/hit, unchanged AP, damage, target pattern, projectile and FX. Melee remains `Staff`.

## Version 0.1.11: Bolt Shot rifle-animation test

The visible Guardian Spear remains `Staff`, preserving the verified melee swing. Only the hidden
bolter profile now advertises `WeaponAnimationStyle.Rifle`, `HoldingType.TwoHanded` and
`IsTwoHanded: true`. `WarhammerOverrideAbilityWeapon` already selects that hidden profile for Bolt
Shot, so the test determines whether Rogue Trader's animation graph also consumes its two-handed
ranged style and aims the visible spear at the target. Projectile origin remains the existing muzzle
locator group; no custom animation clip or runtime combat patch is introduced.

## Version 0.1.10: final facing reversal

The 0.1.9 screenshot confirmed correct scale, pivot, vertical alignment and clean FBX geometry. The
remaining 180-degree turn around the aligned long axis is removed so the blade faces forward. The
muzzle locator is mirrored with the visible bolter side from `(0.06, 0.68, -0.08)` to
`(-0.06, 0.68, 0.08)`. No other visual or gameplay setting is changed.

## Version 0.1.9: FBX pivot and axis correction

The first native-FBX screenshot confirmed clean geometry but exposed an FBX root offset of
`(0.06, 0.22, 0.71)`, measured directly from Unity. The instantiated model transform is now reset to
position zero, identity rotation and unit scale so it stays on the weapon attachment pivot. FBX axis
conversion also mirrors the source diagonal relative to the manual GLB import, so the alignment uses
`+45` rather than `-45` degrees before the established 180-degree facing turn. Mesh, material, scale,
grip animation, muzzle locator and combat data are otherwise unchanged.

## Version 0.1.8: reduced native FBX import

The user reduced the source in Blender and exported `Models/GuardianSpear.fbx` with applied
transforms/modifiers, face smoothing and tangent space. A preserved copy is imported at
`Art/GuardianSpear.fbx`. Unity's native ModelImporter now owns the mesh, normals and tangents;
compression and mesh optimization are disabled to avoid data rewriting. The prefab uses
`GuardianSpear_FBX_Model` instead of the manual GLB mesh chunks and assigns the already proven
Owlcat/Lit material and converted GLB textures to its renderers. Orientation, scale, grip pivot,
muzzle locator and all combat blueprints remain unchanged from 0.1.7.

## Version 0.1.7: aligned facing and 16-bit-safe mesh chunks

The Blender screenshots prove the source GLB is not deformed. Although Unity serialized the imported
mesh with 32-bit indices, the game render path displayed geometry consistent with 16-bit index
wrapping. The importer now preserves every source triangle but partitions the mesh into native Unity
meshes of at most 60,000 vertices, each using 16-bit indices. This duplicates only boundary vertices;
it performs no decimation or shape change. The facing rotation is now applied after diagonal-to-+Y
alignment, keeping the weapon vertical while turning its blade away from the character. The chunked
renderers are children of `GuardianSpear_GLB_Model` and share the same Owlcat/Lit material.

## Version 0.1.6: blade-facing correction

The live 0.1.5 screenshot confirmed the long axis, but showed the blade's lateral projection facing
toward the wielder. `GuardianSpear_GLB_Model` is now rotated 180 degrees around its local Y/long axis,
presenting the opposite model face and moving the blade away from the character. The aligned muzzle
position is mirrored from `(-0.06, 0.68, 0.08)` to `(0.06, 0.68, -0.08)`. Scale, source mesh and combat
blueprints remain unchanged.

## Version 0.1.5: orientation and normal-map correction

The first live GLB screenshots showed that the source diagonal was rotated onto the horizontal axis.
The visual rotation is corrected from `+45` to `-45` degrees around local Z, placing the spear's long
axis on prefab +Y for the Staff animation. The raw glTF tangent-space normal texture is now preserved
as `GuardianSpear_Normal_Source.asset` and repacked to Unity/Owlcat's DXT5nm channel convention in
`GuardianSpear_Normal.asset` (X in alpha, Y in green). The source mesh, scale and gameplay architecture
are unchanged so orientation and shading can be evaluated independently.

## Version 0.1.4: supplied GLB integration

`Pictures/GuardianSpear.glb` is now the source of the visible weapon. The source GLB is preserved
unchanged. Because the Owlcat Unity project has no GLB importer, `Editor/GuardianSpearGlbImporter.cs`
performs a minimal editor-only conversion into native Unity assets. It mirrors glTF's right-handed Z
axis for Unity, reverses triangle winding, and rotates the visual hierarchy -45 degrees around local Z
to align the diagonally authored spear with the prefab's local +Y weapon axis. It does not simplify or
redesign the 565,318-vertex source mesh.

Generated native assets:

- `GuardianSpear_GLB_Mesh.asset`
- `GuardianSpear_BaseColor.asset`
- `GuardianSpear_MetallicRoughness_Source.asset`
- `GuardianSpear_MetallicSmoothness.asset`
- `GuardianSpear_Normal.asset`
- `GuardianSpear_GLB.mat`

The GLB base-colour and normal textures are assigned directly. Its glTF metallic/roughness texture is
also preserved, while a Unity-compatible copy repacks metallic from blue to red and converts roughness
in green to smoothness in alpha. `GuardianSpear_GLB.mat` uses `Owlcat/Lit`; the existing visual-only
runtime binder reconnects that game shader after Owlcat's mod build strips shader programs.

Version 0.1.4 prefab hierarchy:

```text
GuardianSpear_Root                         <- EquipmentOffsets + material binder; grip pivot
├── GuardianSpear_Visual                   <- local Z rotation -45 degrees
│   └── GuardianSpear_GLB_Model            <- GLB mesh + Owlcat/Lit material
├── GuardianSpear_BolterMuzzle             <- FxLocator
└── GripReference_PivotIsRoot              <- authoring marker
```

The muzzle carries locator group `502467bbbcc0471285a4ab6936a285d8` and is currently placed at root
local `(-0.06, 0.68, 0.08)`, measured against the integrated barrel after alignment. Its local rotation
is identity so the firing direction remains prefab +Y. Exact barrel alignment must be confirmed from
an in-game Bolt Shot screenshot.

The visible blueprint still points to the same custom prefab GUID/file ID. The hidden bolter item,
Bolt Shot ability, projectile, VFX and sound references are unchanged. No Deathwatch dependency or
combat Harmony patch was added.

## Result and preserved gameplay architecture

The existing blueprint GUIDs and proven combat architecture are unchanged. Only
`GuardianSpear_Prototype_Item.m_VisualParameters.m_WeaponModel` now points to the custom prefab
`GuardianSpear.prefab` (`1efcf90da19091e498cce6ba0e9fd5bc`, file ID
`5499664329099543335`) instead of the Imperial Staff model.

The visible weapon remains melee, `GuardianSpear_HiddenBolter_Item` remains the actual ranged
profile, and `GuardianSpear_BoltShot_Ability` still uses `WarhammerOverrideAbilityWeapon`, the
existing Bolt projectile/VFX/sound, and `AbilityAmmoLogic.NoAmmoRequired`.

## Asset files

### Mesh assets

- `Art/AuricConstruction.asset` — shaft, pommel, spear blade, bolter casing and guards.
- `Art/GunmetalMechanism.asset` — bolt mechanism, barrel, muzzle collar and magazine housing.
- `Art/DarkGrip.asset` — dark two-handed grip sections.
- `Art/RedDetails.asset` — restrained inset/accent panels.

These are original combined static meshes generated from simple geometry by the editor tool. No
Deathwatch or other mod asset is copied or redistributed. This first pass intentionally uses no
texture maps; material colour, metallic response and roughness/smoothness provide the surface read.

### Materials

Version 0.1.3 adds `Art/SilverBlade.asset` for the custom extruded main blade and side spike.

- `GuardianSpear_Auric.mat`
- `GuardianSpear_Gunmetal.mat`
- `GuardianSpear_Grip.mat`
- `GuardianSpear_Red.mat`
- `GuardianSpear_Blade.mat`

The first two in-game builds proved that both `Owlcat/Lit` and a mod-owned URP shader rendered
magenta. Inspection then identified the official template's `ShaderPreprocessor`, which deliberately
calls `data.Clear()` for every shader during a mod build. Version 0.1.2 therefore includes the small
visual-only `GuardianSpearMaterialBinder`: when the prefab is instantiated, it reconnects the four
material instances to Rogue Trader's already-loaded `Owlcat/Lit` shader via `Shader.Find`. No shader
program is shipped in the mod bundle.

### Prefab and build tooling

- `GuardianSpear.prefab`
- `Editor/GuardianSpearGenerator.cs` (updated editor-only asset/prefab generator)
- `Scripts/GuardianSpearMaterialBinder.cs` and `AstartesCustodes.Runtime.dll` (visual shader rebinding only)
- `Blueprints/GuardianSpear_Prototype_Item.jbp` (updated model reference; existing GUID retained)

The runtime assembly contains only the material binder. No Harmony patch, custom projectile or
custom VFX was added.

## Prefab structure

```text
GuardianSpear_Root                         <- attachment pivot / grip origin
├── GuardianSpear_Visual                   <- corrected visual scale: 0.70
│   ├── AuricConstruction                 <- MeshFilter + MeshRenderer
│   ├── GunmetalMechanism                 <- MeshFilter + MeshRenderer
│   ├── DarkGrip                          <- MeshFilter + MeshRenderer
│   ├── RedDetails                        <- MeshFilter + MeshRenderer
│   └── GuardianSpear_BolterMuzzle        <- FxLocator
└── GripReference_PivotIsRoot             <- authoring marker
```

`GuardianSpear_Root` carries `EquipmentOffsets`. Main-hand and offhand offsets are zero for the first
technical pass, matching the attachment-pivot convention used by weapon prefabs: the root/pivot is
the primary grip reference and all mesh geometry is authored around it. The spear's long axis is
local +Y; the blade and integrated barrel point toward +Y. Final rotation or grip offsets must be
calibrated from an in-game screenshot because the vanilla Staff prefab object is stored in the
game's external bundles and resolves as null in the template's JSON/editor replacement database.

## Muzzle implementation

`FX_Bolter_BoltShot_AbilityVisualFXSettings` uses:

- `m_ProjectileOriginType: CasterWeapon`
- locator group `502467bbbcc0471285a4ab6936a285d8`

The referenced `BlueprintFxLocatorGroup` has an empty `TransformNames` list. Rogue Trader therefore
identifies the point through an `FxLocator` component carrying that group reference, rather than a
magic transform name. `GuardianSpear_BolterMuzzle` carries exactly this component and is positioned
at local `(-0.13, 1.43, -0.105)`, at the end of the visible gunmetal barrel. Both projectile origin and the
existing CasterWeapon muzzle events should resolve to that transform. In-game confirmation remains
required.

## Scaling investigation

Vanilla weapon prefabs support `EquipmentOffsets.raceScaleList`, keyed by `Kingmaker.Blueprints.Race`.
After the first screenshots, the visual hierarchy was reduced to 0.70. The prefab now includes a
`Spacemarine` entry with `WeaponScale: 1.01`; ordinary humanoids use the corrected native scale. The
near-unity Spacemarine entry suppresses Deathwatch's otherwise automatic 1.5x Staff fallback without
materially enlarging an asset that is already authored with superhuman visual mass.

Deathwatch separately patches `UnitViewHandSlotData.OwnerWeaponScale`: human Staff/force weapons
without an existing Spacemarine scale receive 1.5x. Its guard preserves a prefab-provided race scale,
so this spear should use the explicit 1.01x value rather than being inflated again. A future Custodes
race can receive another `raceScaleList` entry once its actual race enum/blueprint integration exists.
No generic runtime scaling system is necessary at this stage.

## Deathwatch main-hand investigation

The exact cause is Deathwatch's `HandSlot.IsItemSupported` postfix. It deliberately lifts the base
two-handed-melee rejection only when `IsPrimaryHand == false`; all supported two-handed melee styles,
including `Staff`, are remapped to `BrutalOneHanded` for the offhand. Changing this weapon's model,
classification or animation style cannot make a genuinely two-handed melee weapon legal in that
main hand.

Changing `IsTwoHanded` to false would evade the rule but would compromise the intended vanilla
two-handed spear behaviour. It was therefore not done. Current Deathwatch compatibility requires
offhand use; main-hand support would require a future optional Deathwatch compatibility change.

## Testing

1. Replace the prior build with `Build/AstartesCustodes.zip`, enable **Guardian Spear Prototype**, and
   restart the game.
2. In ToyBox, open **Search 'n Pick → Items** and add `Guardian Spear Prototype`, or search GUID
   `69a10b7bc7a94c5cb59cd91a6d88d160`.
3. On a vanilla humanoid, equip it normally. Check attachment, +Y orientation, 1.0 scale, primary grip,
   blade clipping and bolter placement. Version 0.1.3 uses the slimmer reference-driven silhouette:
   long narrow shaft, compact integrated bolter, silver curved blade and side spike.
4. Perform the normal melee attack and verify damage still uses the visible profile.
5. Perform **Bolt Shot** and verify damage still uses the hidden bolter profile. Pause/screenshot the
   launch and verify projectile plus muzzle flash originate at `GuardianSpear_BolterMuzzle`, not the
   hand.
6. On Deathwatch, equip it alone in the offhand; mainhand rejection is expected. Repeat melee/Bolt
   Shot and assess the explicit 1.01x Spacemarine scale.

## Known issues / required observations

- Orientation, pivot and grip are technically authored but not yet observed on a live character;
  rotation/offset correction may be required after the first screenshot.
- The first screenshots showed magenta materials and excessive scale. Version 0.1.2 confirmed both
  fixes in game. Version 0.1.3 replaces the blocky prototype geometry with a slimmer, reference-driven
  original silhouette and requires another in-game screenshot for proportion and grip calibration.
- The simple first-pass silhouette has no authored UV textures, baked normal map, wear mask or final
  Custodes ornament.
- The two-hand animation may not place the support hand exactly on the dark grip; no custom animation
  or IK target was added.
- Muzzle locator component and group are verified in the built prefab, but actual CasterWeapon lookup
  must be confirmed in-game.
- Deathwatch mainhand remains intentionally unsupported by Deathwatch's own slot rule.

CUSTOM GUARDIAN SPEAR: PARTIAL SUCCESS

The original custom mesh, Owlcat/Lit materials, standalone prefab, correct bolter locator group,
race scaling and updated installable build are complete. Final success requires the requested live
verification of attachment/orientation and that projectile/muzzle FX resolve to the new locator.
