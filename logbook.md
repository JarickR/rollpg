🎲 DiceArena RPG – Project Overview \& State

🔑 High-Level Concept



DiceArena is a custom pixel/RPG project (Dungeons and Dragons inspired) with:



Loadout Screen → player selects class, Tier1/Tier2 spells.



Battle Screen → party members instantiated with selected abilities.



Engine Layer (DiceArena.Engine.Core) → defines game state (Game, PartyMember, etc.).



Godot Layer (DiceArena.Godot) → UI nodes (LoadoutScreen, BattleRoot, HeroCard, etc.) that render and interact with engine state.



Bridging → LoadoutToBattleBridge wires Loadout → Battle, passing the Engine Game.



Icons for spells/classes are retrieved from a centralized IconLibrary, with fallbacks to transparent textures.



📂 Current File Layout \& Descriptions

Engine Layer



Scripts/Engine/Core/Game.cs



Holds canonical engine-side Game state: party members, size, spells.



Defines PartyMember (the logical representation of a hero and their tier picks).



Scripts/Engine/Loadout/LoadoutScreen.cs



A Control UI that emits Finalized(Game) when the player presses Finalize.



Keeps a private Engine.Core.Game instance to accumulate selections.



Inspector FinalizeButtonPath wires to a Button.



Scripts/Engine/Loadout/IconLibraryBridge.cs / IconLibraryShim.cs



Adaptors that make sure engine code can call into Godot.IconLibrary.



Expect IconLibrary.GetClassTexture and IconLibrary.GetSpellTexture.



Godot Layer



Scripts/Godot/LoadoutToBattleBridge.cs



Bridge node that:



Shows LoadoutScreen first.



Subscribes to LoadoutScreen.Finalized.



Hides Loadout, shows BattleRoot.



Calls \_battle.ApplyFromGame(engineGame).



Scripts/Godot/BattleRoot.cs



Root container for battle screen.



Inspector-exported paths: FriendlyRootPath, EnemyRootPath, LogRootPath.



Caches \_friendlyRoot, \_enemyRoot, \_log.



Provides:



ApplyFromGame(Engine.Core.Game) – consumes engine game state.



Logging helpers (LogInfo/LogWarn/LogError) via BBCode in RichTextLabel.



Currently clears/resets containers and logs party size.

(TODO: Hydrate hero cards into \_friendlyRoot.)



Scripts/Godot/HeroCard.cs



Visual representation of a hero slot.



Uses IconLibrary.GetClassTexture + GetSpellTexture to populate sprites.



Scripts/Godot/EnemyCard.cs



Analogous to HeroCard, but for enemies.



Scripts/Godot/IconLibrary.cs



Static texture loader.



Provides:



Transparent1x1 (safe placeholder).



GetClassTexture(className, size).



GetSpellTexture(spellName, tier, size).



Uses ResourceLoader.Load<Texture2D> with a set of guess paths.



Returns fallback if no match.



📝 Lessons Learned (So Far)



⚠️ Namespace collisions:

Both DiceArena.Godot.Game and DiceArena.Engine.Core.Game existed → always alias with:



using EngineGame = DiceArena.Engine.Core.Game;





and type method signatures explicitly.



⚠️ Missing members:

Initial references like Tier1A/Tier1B were removed → replaced with canonical PartyMember fields defined in Engine.Core.



⚠️ Godot API drift:



NodePath.Empty doesn’t exist in Godot 4 → replaced with new NodePath().



RichTextLabel.PushWarning/PushError/PushBold don’t exist → replaced with AppendText + BBCode formatting (\[color=yellow], \[color=red], \[b]).



Image.Create(...) is obsolete → replaced with Image.CreateEmpty(...).



✅ Bridge wiring:

Finalize button triggers Finalized(Game) → LoadoutToBattleBridge → BattleRoot.ApplyFromGame(...).



✅ UI state toggling:

Only one screen visible at a time. Loadout → finalize → hide Loadout/show Battle.



🏗️ Current Functional State



Compile-Ready:

Project compiles without ambiguous Game errors, obsolete API warnings, or missing members.



Loadout Screen:

Appears, shows finalize button.

Still needs proper icon buttons for selecting class/Tier1/Tier2 spells.



Finalize:

Correctly fires, passes Engine.Core.Game into bridge.



Battle Screen:

Appears, logs receipt of game.

Does not yet instantiate hero/enemy cards (TODO).



Icons:

IconLibrary now contains both GetClassTexture and GetSpellTexture.

If icons not found, defaults to transparent placeholder.



📌 Next Steps / TODOs



Populate LoadoutScreen with interactive icon-based selection:



Replace OptionButtons with TextureButtons.



Use IconLibrary.GetClassTexture and GetSpellTexture.



Update \_game selections accordingly.



Hydrate BattleRoot:



Instantiate HeroCard nodes for each PartyMember.



Display class icon + Tier1/Tier2 spell icons.



Expand PartyMember:



Add fields for selected spells/tiers (if not already done).



Ensure serialization from Loadout → Battle.



Enemy Setup:



Mirror HeroCard logic with EnemyCard.



Content Assets:



Ensure icon PNGs exist in expected folders:



res://Content/Icons/classes/



res://Content/Icons/Tier{tier}Spells/



🚀 Summary



Engine → Godot pipeline is now wired (Loadout → Bridge → Battle).



Core compile/runtime blockers (namespaces, obsolete APIs, missing methods) have been resolved with aliases, overloads, and safe fallbacks.



Project is ready for UI enrichment (interactive selection, card instantiation).









# RollPG – Repository Guide & File Descriptions

_Last updated: {{today}}_

This document orients a new contributor to the project structure, how to build/run, how the two main screens (Loadout and Battle) connect, and where important scripts live.

---

## Quick Start

### Requirements
- **Godot 4.x (C#/.NET)** with the Mono build (install the .NET-enabled editor).
- **.NET SDK 8.0** (project targets .NET 8).

### Build & Run
1. Open `project.godot` in Godot 4 (Mono).
2. Let Godot restore/build the C# project (`RollPG.csproj`).
3. Run the project from the editor ▶️.

If you see import warnings on textures, let the import pipeline finish. Do a clean build if the C# cache (`.godot/mono/temp/obj`) gets noisy.

---

## High-Level Gameplay Loop

- **Loadout Screen**: Player selects a party composition (class + Tier1/Tier2 spells per hero).
- **Battle Screen**: Uses the selected party to render hero cards and enemies and drives the combat loop. Icons for classes and spells are loaded via the Icon Library.

Data flows one-way from Loadout → Battle for a given run, then Battle mutates runtime state (damage, effects, logs).

---

## Scenes

- **`Scenes/GameRoot.tscn`**  
  Entry/root scene; bootstraps UI flow and owns top-level nodes used by screens.  
- **`Scenes/HeroCard.tscn`**  
  UI fragment for showing one hero: class icon + Tier1/Tier2 spell icons, and name.  
- **`Scenes/EnemyCard.tscn`**  
  UI fragment for rendering a single enemy.

---

## Namespaces & Where Things Live

> (File map derived from the current checkout and folder listing.) :contentReference[oaicite:2]{index=2} :contentReference[oaicite:3]{index=3}

### Godot glue (nodes, scenes, UI)
- `Scripts/Godot/BattleRoot.cs` — **DiceArena.Godot**  
  High-level battle UI driver: finds hero panel/log nodes, renders party to hero cards, writes log lines.
- `Scripts/Godot/HeroCard.cs` — **DiceArena.Godot**  
  A `Control` for one hero’s display (class icon + Tier1/Tier2 icons + label). Provides `SetHero(...)` & helpers.
- `Scripts/Godot/EnemyCard.cs` — **DiceArena.GodotUI**  
  Renders an enemy card (name, HP, icon).
- `Scripts/Godot/EnemyPanel.cs` — **DiceArena.Godot**  
  Groups enemy cards on the battle screen.
- `Scripts/Godot/IconLibrary.cs` — **DiceArena.Godot**  
  Loads and caches class & spell textures from `res://` by name/tier, returns `Texture2D` used by cards.
- `Scripts/Godot/IconFlushOnBoot.cs` — **DiceArena.Godot**  
  Clears icon cache at app start (useful when iterating imports).
- Other Godot helpers: `UiScroll.cs`, `RollPopup.cs`, `HoverBubble.cs`, etc.

### Engine (game logic, loadout system, models)
- `Scripts/Engine/Loadout/LoadoutScreen.cs` — **DiceArena.Engine.Loadout**  
  Builds the loadout UI (class chooser, Tier1/Tier2 selectors, finalize). Emits events when selection changes.
- `Scripts/Engine/Loadout/*` (Compat, Core, System, Models, MemberCard, Icon*):  
  Helpers for reusable icon tiles, tooltips, and data transfer objects used by the UI.
- `Scripts/Engine/*` (GameState, Models, Dice, Enemy, Spells, etc.):  
  Core game types and future combat logic expansion.

### Data Layer
- `Scripts/Data/DataModels.cs`, `Scripts/Data/GameDataRegistry.cs` — **DiceArena.Data**  
  DTOs + lookups for content (classes, spells) loaded from JSON:
  - `Content/classes.json`
  - `Content/spells.json`

---

## Content & Assets

- **Icons** (class + spells) live under `res://` and are imported to `.ctex` under `.godot/imported/…`.  
  You’ll see many `*-Tier1.png`, `*-Tier2.png`, and class logo textures. The `IconLibrary` resolves these by name and tier. :contentReference[oaicite:4]{index=4}
- To adjust how big they render, prefer **Import Settings** (resize) or `TextureRect.Expand = true` + `StretchMode = KeepAspectCentered` in the UI.

---

## Loadout → Battle: Data Flow

1. **Selection on LoadoutScreen**  
   - Player picks `ClassId` and up to two spells (Tier1, Tier2).  
   - `LoadoutScreen` raises a **Finalized** signal with a collection of party members (name, class, tier1, tier2).
2. **Bridge (LoadoutToBattleBridge)**  
   - Listens to the `Finalized` event.  
   - Stores the chosen party (in a simple `Engine.Core.Game`/state object or a transient payload) and switches to the Battle view.
3. **BattleRoot**  
   - Reads the current party and calls `HeroCard.SetHero(name, classId, tier1Spell, tier2Spell)`.  
   - Each `HeroCard` uses `IconLibrary` to resolve:  
     - `GetClassTexture(classId, size)`  
     - `GetSpellTexture(spellName, tier, size)`  
   - If a spell is `null` or empty, card shows a 1×1 transparent texture as a placeholder.

> If you change file/texture names, update `IconLibrary`’s lookup rules (class IDs, spell names) accordingly.

---

## Signals & Events

- **LoadoutScreen**  
  - `event Action<MemberLoadout[]> Finalized;` (or project’s current equivalent)  
    Emitted when the user presses the Finalize/Continue button.

- **BattleRoot**  
  - Consumes the party and renders to hero cards; logs to a `RichTextLabel`.

- **Icon Cache**  
  - Optionally cleared on startup by `IconFlushOnBoot` to reflect newly imported textures.

---

## Coding Conventions (C# in Godot 4)

- **Node paths** exported as `NodePath` with optional inspector wiring.  
- Use `GetNodeOrNull<T>(path)` and guard against `null` — never assume Inspector wiring.
- Prefer `StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered` on `TextureRect` that render icons.
- Avoid Godot-3 API (e.g., `Image.Lock/Unlock`); use Godot-4 equivalents (`Image` methods and `ImageTexture.CreateFromImage`).
- Don’t rely on `Control.SizeFlags` with raw ints; cast or use the enum (`Control.SizeFlags.Fill | Control.SizeFlags.Expand`).

---

## File-by-File Highlights (selection)

> These are the files a new contributor will touch first. The full catalog lives in `FILE-LIST.txt`. :contentReference[oaicite:5]{index=5}

- `Scripts/Godot/HeroCard.cs`  
  - Public API:
    - `SetHero(string heroName, string classId, string? tier1SpellName, string? tier2SpellName)`
    - `SetClass(string classId)`, `SetTier1(string? spell)`, `SetTier2(string? spell)`
  - Looks up child nodes (by exported `NodePath` or common names like `"ClassIcon"`, `"T1Icon"`, `"T2Icon"`, `"Name"`).
  - Applies textures via `IconLibrary`.

- `Scripts/Godot/IconLibrary.cs`  
  - Static cache of `Texture2D`s keyed by `(kind, name, tier, size)`.  
  - `GetClassTexture(classId, size)` and `GetSpellTexture(spellName, tier, size)`.  
  - `Transparent1x1` as fallback (created with `Image.CreateEmpty(1,1,…)` in Godot 4).

- `Scripts/Engine/Loadout/LoadoutScreen.cs`  
  - Builds class/spell pickers (often with `IconTile` or `Button`) and assigns `HintTooltip` via helper extensions.  
  - Emits `Finalized` with `MemberLoadout[]`.

- `Scripts/Godot/BattleRoot.cs`  
  - Reads party from the bridge / game state and populates hero panel with `HeroCard` instances.  
  - Writes to battle log (`RichTextLabel`).

---

## Asset & Naming Rules

- **Classes**: use canonical IDs (e.g., `Barbarian`, `Bard`, …) that match class icon filenames (e.g., `Barbarian.png`, `Bard.png`).  
- **Spells**: each spell has a display name and a tier; icon files follow the pattern:
  - `SpellName-t1.png`, `SpellName-t2.png`, `SpellName-t3.png`  
  Many imported variants exist under `.godot/imported/*.ctex` (do not edit those directly). :contentReference[oaicite:6]{index=6}
- **Case sensitivity**: Godot paths are case-sensitive on some platforms. Keep names consistent.

---

## Troubleshooting Notes (lessons learned)

- **Godot 4 API**: Replace Godot 3 calls (`Image.Lock/Unlock`, `Image.Create`) with 4.x equivalents (`Image.CreateEmpty`, direct pixel set if needed).  
- **SizeFlags**: Don’t assign raw ints; use `Control.SizeFlags` flags or explicit casts.  
- **NodePath**: Godot 4 doesn’t have `NodePath.Empty` or `.IsEmpty()`. Use `path == default` or `path.IsEmpty` extension only if _you_ define it; otherwise, guard by `string.IsNullOrEmpty(path.ToString())`.  
- **Ambiguous namespaces**: We separate `DiceArena.Godot` (UI) and `DiceArena.Engine.*` (logic). When conflicts arise (e.g., `Game`), qualify with full namespace.  
- **Icon scaling**: Prefer texture import size over code-side scaling. If mixing sizes, ensure `TextureRect.Expand = true` and an appropriate `StretchMode` are set.

---

## Glossary

- **Party Member**: A player-controlled hero (name, class, Tier1/Tier2 spells).
- **Tier1 / Tier2**: Spell tiers; cards render icons for each selected spell.
- **IconLibrary**: Texture loader + cache for class & spell icons.
- **Loadout Screen**: Pre-battle UI where party is composed.
- **Battle Screen**: Main combat presentation (hero cards, enemies, log).

---

## Changelog (living)

- **2025-09-23** — Added this guide: build/run, scenes overview, Godot 4 API notes, loadout→battle flow, asset conventions, and troubleshooting. (Matches current repo contents and file catalog.) :contentReference[oaicite:7]{index=7}

---

## See Also

- **FILE-LIST.txt** — exhaustive directory listing for the repo (auto-generated). :contentReference[oaicite:8]{index=8}
- **Content/** — `classes.json` and `spells.json` for static data. :contentReference[oaicite:9]{index=9}


