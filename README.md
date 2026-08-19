# Armoury: Adeptus Custodes

A Custodes armoury mod for *Warhammer 40,000: Rogue Trader*.

Version 1.3.1 includes two complete weapons: the **Guardian Spear**, a two-handed
hybrid melee and bolter weapon, and the **Sentinel Sword**, a defensive one-handed
power sword. Both weapons have custom models and inventory icons, six progressively
stronger variants, and automatically upgrade with their wielder from level 1 to 55.
It also includes six progressing variants of the **Praesidium Shield** with custom
abilities and visuals.

No Deathwatch, MicroPatches, Harmony, or other mod dependency is required.

## Installation

Download the latest release ZIP, install it with Modfinder for Rogue Trader, and
enable **Armoury: Adeptus Custodes**.

## Current version

`1.3.1`

Version 1.3.1 fixes the documented cooldowns of the Sentinel Sword's Activate Power
Field and the Praesidium Shield abilities. Version 1.3.0 added six progressing variants of the
Praesidium Shield: Auric Bastion protects nearby allies with individual force shields,
while Null Shockwave suppresses Minor and Major Psychic Powers in its large null zone.

## Automatic weapon progression

Only the first variant of a weapon needs to be obtained. Outside combat, it is
automatically replaced when its wielder reaches levels 16, 26, 36, 44, and 50.

| Levels | Guardian Spear | Sentinel Sword |
| --- | --- | --- |
| 1-15 | Custodian's Vigil | Custodian's Edge |
| 16-25 | Auric Watch | Auric Talon |
| 26-35 | Praetorian's Oath | Praetorian's Answer |
| 36-43 | Wrath of the Ten Thousand | Blade of the Ten Thousand |
| 44-49 | Voice of the Golden Throne | Judgement of Terra |
| 50-55 | The Emperor's Vengeance | The Emperor's Final Decree |

Upgrades never occur during combat. If a level is changed through ToyBox, the
replacement is applied at the next safe progression check outside combat.

## Guardian Spear

The Guardian Spear is a complete two-handed hybrid weapon with a shared ammunition
pool and four attacks:

- **Strike** — focused melee attack
- **Guardian Cleave** — sweeping melee attack
- **Bolt Shot** — single ranged bolter shot
- **Bolt Burst** — multi-shot ranged burst

Its melee and ranged damage, armour penetration, burst size, magazine capacity,
Weapon Skill, Ballistic Skill, parry chance, critical chance, and critical damage
scale across the six variants. It is classified as a power sword so applicable
power-sword talents can interact with it.

## Sentinel Sword

The Sentinel Sword is a one-handed power sword with a defensive profile. Its Weapon
Skill, parry chance, dodge chance, damage, and armour penetration improve through all
six variants. Its abilities are:

- **Strike** — standard single-target attack
- **Cleave** — area attack dealing 90% of Strike damage
- **Onslaught** — wider area attack dealing 80% of Strike damage
- **Activate Power Field** — 0 AP self-buff that adds bonus weapon damage; duration
  and bonus damage increase with each variant, with a 7-round cooldown
- **Sentinel Wave** — 1 AP force attack with a range of 5 cells and a 2-round cooldown

The final variant's Power Field remains active until combat ends once activated.

## Obtaining the weapons

Version 1.3.1 does not yet distribute the weapons through a quest, vendor, or
starting equipment. ToyBox is currently required. Search for the first variant and
add it to the inventory:

| Weapon | ToyBox name | Blueprint ID |
| --- | --- | --- |
| Guardian Spear | `Custodian's Vigil` | `69a10b7bc7a94c5cb59cd91a6d88d160` |
| Sentinel Sword | `Custodian's Edge` | `94d7497e0b1941c1910a6b29ed8911c2` |
| Praesidium Shield | `Praesidium Shield Prototype` | `e44e715029c9490ab7df80ad4366996b` |

## Planned additions

The following Custodes equipment is planned for future updates, using the same
six-variant level-scaling concept:

- Castellan Axe
- Praesidium Shield variants and progression

A separate future mod is also planned around playing an Adeptus Custodes character
with dedicated armour and related character features.
