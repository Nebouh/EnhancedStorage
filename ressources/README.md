# Enhanced Storage

A mod for **Schedule I** that improves and customizes the storage capacity of various storage racks in the game.

Created by **Nebouh** — powered by **MelonLoader** and **Harmony**.

---

## 📦 Features

- Increases the slot count and number of display rows for storage racks.
- Allows full customization per storage type via configuration.
- Includes a global cap for total slot count to maintain balance.
- Uses MelonPreferences for user-configurable settings.

---

## 🛠️ Technical Details

This mod dynamically patches `Storage Methods` using Harmony to modify internal values such as:

- `SlotCount`
- `DisplayRowCount`

These are applied based on user configuration and capped globally using `GameMaxSlots`.

Supported storage types:

| Storage Type          | Default | Configurable |
|-----------------------|---------|--------------|
| Small Rack            | 4x1     | ✅            |
| Medium Rack           | 6x1     | ✅            |
| Large Rack            | 8x2     | ✅            |
| Wall Mount Shelf      | 4x1     | ✅            |
| Display Cabinet       | 4x1     | ✅            |
| Wood Square Table     | 3x1     | ✅            |
| Metal Square Table    | 3x1     | ✅            |
| Safe                  | 8x2     | ✅ NEW        |
| Coffee Table          | 3x1     | ✅ NEW        |

---

## ⚙️ Configuration

Settings are stored in the following file: `./UserData/MelonPreferences.cfg`

Note:
- "Slots" defines the number of inventory slots added to the object.
- "Rows" defines how these slots are visually organized (split into rows).

To enable the mod and apply custom settings, modify the configuration like this (Default settings):
```ini
["EnhancedStorage"]
EnableMod = true
UseDefaultSettings = false
GameMaxSlots = 20
SmallRackSlots = 8
SmallRackRows = 2
MediumRackSlots = 12
MediumRackRows = 2
LargeRackSlots = 18
LargeRackRows = 3
WallMountShelfSlots = 12
WallMountShelfRows = 2
DisplayCabinetSlots = 8
DisplayCabinetRows = 2
WoodSquareTableSlots = 9
WoodSquareTableRows = 3
MetalSquareTableSlots = 9
MetalSquareTableRows = 3
SafeSlots = 18
SafeRows = 3
CoffeeTableSlots = 6
CoffeeTableRows = 1
```