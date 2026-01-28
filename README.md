# Dungeon of the Archdemon (Rogue-like)

A challenging, turn-based terminal Rogue-like built in C#. Brave 15 floors of procedurally generated danger, find legendary treasures, and defeat the Archdemon to claim victory.

## How to Play
1. **Choose your Hero:** Select between the Warrior, Archer, or Mage.
2. **Explore:** Move through the fog of war to find the exit (▼).
3. **Fight:** Defeat enemies to gain XP and level up.
4. **Survive:** Manage your health and potions. You only have one life!

## Controls
| Key | Action |
| :--- | :--- |
| **Arrow Keys** | Move / Attack / Break Walls |
| **Spacebar** | Use Class Special Ability (Costs HP) |
| **H** | Drink a Healing Potion (+45 HP) |
| **B** | Open the Bestiary (Monster Info) |
| **R** | Resume a saved game (from Main Menu) |

## Classes & Abilities
* **Warrior:** High HP. Special: **Radar** (Reveals nearby traps).
* **Archer:** High DMG. Special: **Eagle Eye** (Reveals a large area of the map).
* **Mage:** Very High DMG. Special: **Teleport** (Blinks to a random safe location).

## Features
* **Procedural Levels:** Every floor is different.
* **Secret Rooms:** Break suspicious walls to find Legendary Treasures (Sword, Armor, or Ring).
* **Save System:** Your progress is automatically saved at the start of each floor.
* **Boss Fights:** Encounter a major Boss every 5 floors.
* **Fog of War:** Strategic exploration with limited visibility.
3. Run the following command for a standalone `.exe`:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
