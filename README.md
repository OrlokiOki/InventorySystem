# Inventory System
---
## Overview
A modular 2D inventory system built with **C# and MonoGame**. Features a fully interactive inventory with drag-and-drop, an alchemy crafting tab, item tooltips and a pixel art UI with custom textures.

---
## Requirements
- **MonoGame 3.8+**
- **.NET 8 or later**
- Asset files: `Pixelta.ttf`, `item_default.png`, `ui_*.png` textures

---
## Run
Open the solution in **Visual Studio**, build the Content project first, then run.

---
## Gameplay
- **E** — Open / close inventory
- **Escape** — Quit
- **Drag item → equipment slot** — Equip
- **Drag equipped item → grid** — Unequip
- **Drag item → mix slot** — Place in alchemy mixer
- **Mix button** — Craft from the two mix slots
- **Click recipe** — Craft directly if ingredients are available
- **Mouse wheel** — Scroll inventory grid

---
## Features
- **Inventory tab** — 6-column scrollable grid, drag-and-drop equip/unequip, search bar, sort by name or value
- **Alchemy tab** — two-slot mixing station, result preview, discoverable recipes, timed success/fail messages
- **Equipment slots** — Head, Necklace, Chest, Ring, Gauntlets, Boots, Right Hand, Left Hand
- **Tooltips** — hover any item for name, description, stats and effects
- **Item icons** — unique texture per item, fallback to `item_default.png` if missing

---
## Project Structure
```
InventorySystem/
├── Content/                   # Pipeline assets (textures, fonts)
├── Game1.cs                   # Entry point, state routing, test data
├── GameState.cs               # DarkScreen / Inventory enum
├── DarkScreenState.cs         # Opening screen
├── InventoryState.cs          # Inventory tab — UI, drag-drop, sort, search
├── AlchemyState.cs            # Alchemy tab — mixing, recipe list, tooltips
├── Tooltip.cs                 # Hover and timed tooltip renderer
├── Character.cs               # Base class — stats, inventory helpers, equip slots
├── PC.cs                      # Player — equip, unequip, sort, craft, known recipes
├── Item.cs                    # Abstract base for all items (id, name, value, icon)
├── Weapon.cs                  # Damage + optional enchantment
├── Armor.cs                   # Defense + ArmorSlot + optional enchantment
├── Accessory.cs               # Ring / Necklace + intrinsic effect
├── Consumable.cs              # Stackable, effect with magnitude and duration
├── EnchantingStone.cs         # Applies an Effect to a Weapon or Armor
├── Effect.cs                  # EffectType, TargetType, Magnitude, Duration
├── Recipe.cs                  # Two ingredients → one Consumable result
├── InventorySlot.cs           # Item wrapper with quantity for the bag
└── Enums.cs                   # ArmorSlot, AccessorySlot, EffectType, TargetType
```

---
## Class Flow
`Game1` owns a `PC` and two states. On `E`, it transitions from `DarkScreenState` to `InventoryState`. Tab clicks inside `InventoryState` route the active tab index — when tab 1 is active, `Game1` delegates `Update` and `Draw` to `AlchemyState` and draws only the tab strip on top via `DrawTabsOnly`.

`PC` inherits from `Character`, which manages the raw inventory list and equip slots. `PC` exposes public methods for equipping, unequipping, sorting and crafting. Crafting checks `AllRecipes` (all game recipes), consumes ingredients, and calls `LearnRecipe` if the result is new — adding it to `KnownRecipes` shown in the alchemy tab.

Items inherit from `Item` and carry an `Icon` texture assigned at startup by `Game1.LoadItemIcons()`, which maps item names to Content asset names (`"Iron Sword"` → `item_iron_sword`). Missing icons fall back to `item_default`.

---
## Adding Item Icons
Name the PNG `item_youritemname.png` (lowercase, spaces → underscores), place it in `Content/`, add a `#begin` entry to `Content.mgcb` and rebuild Content.

---
## License
Educational project for learning purposes. Made by OrlokiOki.
