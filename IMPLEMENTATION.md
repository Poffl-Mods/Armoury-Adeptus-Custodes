# Guardian Spear Prototype implementation

## Inspection findings

- The target workspace was empty. The adjacent `AstartesArmoury_RT_MOD` project established the
  local Owlcat `.jbp`, localization, modification-asset, generator, and build conventions.
- The adjacent Deathwatch source is available as a read-only reference. Its
  `AstartesPhysiology_Feature` contains `TwoHandedWeaponsInOneHand` support for `Staff` (among other
  styles), converting the off-hand presentation to `BrutalOneHanded`. Deathwatch itself was not
  modified and this prototype has no hard dependency on it.
- The installed Owlcat modification template and its blueprint database verified the requested
  Vindictor, staff, klaive, bolter-FX, and projectile references. The task text omitted the final
  `3` from the hidden Vindictor melee weapon GUID: the verified GUID is
  `91ab9da13b8848aab46bd885a0199db3`.
- Full local blueprint exports confirmed that `VindictorFlamer_MeleeSingle_Ability` adds both
  `WarhammerOverrideAbilityWeapon` and `AbilityAmmoLogic` to a weapon attack.

## New blueprints

| Blueprint | GUID | Prototype/reference |
| --- | --- | --- |
| `GuardianSpear_Prototype_Item` | `69a10b7bc7a94c5cb59cd91a6d88d160` | `ImperialStaff_Item` (`993996a4c0a24463aa400b9441d4caa8`) |
| `GuardianSpear_HiddenBolter_Item` | `94fb9c35f58b4442bb7b17f660257f2f` | `AstartesBoltPistol_Item` (`5e1bae4c2c7e4bd99411173f8dbe74f0`) |
| `GuardianSpear_BoltShot_Ability` | `747e419a3f9c43579f51b27f41e88b35` | standard bolt single shot (`6a7f0c4523c34de7829c088556b62f11`) |

## Mechanics

The visible item remains a two-handed `Staff`-animated melee weapon. Its first weapon slot uses the
Imperial Staff's normal single-target melee ability. Test damage is 18–24 with 15% penetration.

Its second weapon slot exposes `GuardianSpear_BoltShot_Ability`. That ability retains the vanilla
`WarhammerAbilityAttackDelivery` with `WeaponAttack: Ranged`, then adds
`WarhammerOverrideAbilityWeapon` pointing to `GuardianSpear_HiddenBolter_Item`. The hidden profile is
a Bolt-family ranged weapon with 17–22 damage, 20% penetration, range 15, and no visible model. It is
marked unusable as a standalone item and unlootable. Version 0.1.22 gives the visible hybrid item an
18-round magazine; Bolt Shot consumes one round and Bolt Burst uses the hidden profile's six-round rate of fire.
Because `WarhammerOverrideAbilityWeapon` redirects vanilla consumption to a visual-less profile, the runtime
pose controller also routes the committed attack's cost to `AbilityData.SourceWeapon.CurrentAmmo` exactly once.

The ability and weapon slot reuse `FX_Bolter_BoltShot_AbilityVisualFXSettings`
(`afde0e8c0c9848deba8e38a1279ee7df`); its referenced vanilla projectile is
`c83759d106dbcb44593c2090aa6d5d95`. No custom projectile, VFX, sound, or runtime C#/Harmony code is
included. `GuardianSpearGenerator.cs` is editor-only build tooling.

## Expected visual limitations

The original placeholder has now been replaced by the first custom Guardian Spear prefab. See
`CUSTOM_ASSET.md` for its mesh/material hierarchy, verified locator-group implementation, scaling
investigation, current Deathwatch offhand requirement, and the new in-game visual test.

## In-game test

1. Install `Build/AstartesCustodes.zip` with ModFinder, or extract it into the game's user
   `Modifications` directory.
2. Enable **Guardian Spear Prototype** in the in-game Mods menu and restart if prompted.
3. Load a Deathwatch/Astartes test character (preferred) or another character able to equip a
   two-handed staff.
4. In ToyBox, open **Search 'n Pick**, select the **Items** category, search for
   `Guardian Spear Prototype` (or GUID `69a10b7bc7a94c5cb59cd91a6d88d160`), and add one copy to the
   player inventory. ModFinder itself has no item-spawn console command.
5. Equip only the visible Guardian Spear Prototype. Confirm that the staff model appears and that
   the normal melee attack and **Bolt Shot** are both present.
6. In combat, damage an enemy with the melee attack, then damage an enemy at range with **Bolt
   Shot**. Confirm the melee tooltip/combat log reports 18–24 base damage and the shot reports 17–22,
   proving that the two attacks use separate profiles.
7. Record where the projectile and muzzle flash originate. Also verify that Bolt Shot does not
   consume ammunition.

## In-game verification

The custom model, attachment, melee attack, Bolt Shot, projectile origin and muzzle result have now
been verified successfully in game. Version 0.1.21 uses the game's Single Shot, Heavy Bolter Burst
and two-handed Cleave icons. Bolt Burst shares Bolt Shot's horizontal aiming pose, while Guardian
Cleave routes animation selection through a hidden vanilla greatsword profile.
