# RollPG – File Catalog

Generated: 2025-09-23T13:58:30

| File | Kind | Namespace | Role | Main Type | Purpose |
|---|---|---|---|---|---|
| .godot\mono\temp\bin\Debug\RollPG.deps.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| .godot\mono\temp\obj\Debug\.NETCoreApp,Version=v8.0.AssemblyAttributes.cs | C# |  | Misc |  |  |
| .godot\mono\temp\obj\Debug\RollPG.AssemblyInfo.cs | C# |  | Misc |  |  |
| .godot\mono\temp\obj\Debug\RollPG.sourcelink.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| .godot\mono\temp\obj\project.assets.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| .godot\mono\temp\obj\RollPG.csproj.nuget.dgspec.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| Content\classes.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| Content\spells.json | JSON |  | Misc |  | Static game content (classes, spells, etc.). |
| Scenes\EnemyCard.tscn | Godot Scene |  | Misc |  | Godot scene. |
| Scenes\GameRoot.tscn | Godot Scene |  | Misc |  | Godot scene. |
| Scenes\HeroCard.tscn | Godot Scene |  | Misc |  | Godot scene. |
| Scripts\Data\DataModels.cs | C# | DiceArena.Data | Game data layer (bridges Data <-> Engine) | class ClassDef | Type definitions (DTOs) used across the project. |
| Scripts\Data\GameDataRegistry.cs | C# | DiceArena.Data | Game data layer (bridges Data <-> Engine) |  | Caches + provides lookups for models at runtime. |
| Scripts\Engine\CombatSystem.cs | C# | DiceArena.Engine | Misc |  |  |
| Scripts\Engine\Content\ContentDatabase.cs | C# | DiceArena.Engine.Content | Engine.Content — data models & loaders |  | Loading/parsing JSON content into models. |
| Scripts\Engine\Content\Models.cs | C# | DiceArena.Engine.Content | Engine.Content — data models & loaders |  | Type definitions (DTOs) used across the project. |
| Scripts\Engine\Dice.cs | C# | DiceArena.Engine | Misc |  |  |
| Scripts\Engine\EncounterTier.cs | C# | DiceArena.Engine | Misc | enum EncounterTier |  |
| Scripts\Engine\Enemy.cs | C# | DiceArena.Engine | Misc | class Enemy |  |
| Scripts\Engine\Face.cs | C# | DiceArena.Engine | Misc | enum FaceType |  |
| Scripts\Engine\GameState.cs | C# | DiceArena.Engine | Misc | class GameState | High-level Godot nodes or gameplay drivers. |
| Scripts\Engine\Hero.cs | C# | DiceArena.Engine | Misc | enum DefenseType |  |
| Scripts\Engine\Loadout\IconLibraryBridge.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  | Loads, resizes, and caches icon textures. |
| Scripts\Engine\Loadout\IconLibraryShim.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  | Loads, resizes, and caches icon textures. |
| Scripts\Engine\Loadout\IconTile.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder | class IconTile | Reusable icon button with tooltip. |
| Scripts\Engine\Loadout\LoadoutCompat.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder | class MemberLoadout |  |
| Scripts\Engine\Loadout\LoadoutCore.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  |  |
| Scripts\Engine\Loadout\LoadoutMemberCard.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder | class LoadoutMemberCard | Card UI for a single party member. |
| Scripts\Engine\Loadout\LoadoutModels.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder | enum DieFaceKind | Type definitions (DTOs) used across the project. |
| Scripts\Engine\Loadout\LoadoutScreen.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder | class LoadoutScreen | Builds and renders the Loadout UI, handles events. |
| Scripts\Engine\Loadout\LoadoutScreenExtensions.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  | Builds and renders the Loadout UI, handles events. |
| Scripts\Engine\Loadout\LoadoutScreenSignals.cs | C# | DiceArena.GodotUI | Engine.Loadout — UI + logic for party builder | class LoadoutScreen | Builds and renders the Loadout UI, handles events. |
| Scripts\Engine\Loadout\LoadoutSystem.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  |  |
| Scripts\Engine\Loadout\Logging.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  |  |
| Scripts\Engine\Loadout\RichTextLabelExtensions.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  | C# extension helpers to tidy UI code. |
| Scripts\Engine\Loadout\TooltipBuilder.cs | C# | DiceArena.Engine.Loadout | Engine.Loadout — UI + logic for party builder |  | Builds tooltip header/body text. |
| Scripts\Engine\Models.cs | C# | DiceArena.Engine | Misc | class Hero | Type definitions (DTOs) used across the project. |
| Scripts\Engine\Spell.cs | C# | DiceArena.Engine | Misc | enum SpellKind |  |
| Scripts\Engine\SpellFactory.cs | C# | DiceArena.GameData | Misc |  |  |
| Scripts\Engine\Spells.cs | C# | DiceArena.Engine | Misc |  |  |
| Scripts\GlobalUsings.cs | C# |  | Misc |  |  |
| Scripts\Godot\BattleLogPanel.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring | class BattleLogPanel | High-level Godot nodes or gameplay drivers. |
| Scripts\Godot\BattleRoot.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class BattleRoot | High-level Godot nodes or gameplay drivers. |
| Scripts\Godot\DebugTextEditScanner.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring |  |  |
| Scripts\Godot\EnemyCard.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring | class EnemyCard |  |
| Scripts\Godot\EnemyPanel.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class EnemyPanel |  |
| Scripts\Godot\Game.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class Game | High-level Godot nodes or gameplay drivers. |
| Scripts\Godot\HeroCard.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class HeroCard | UI for hero details / selection. |
| Scripts\Godot\HeroPanel.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class HeroPanel | UI for hero details / selection. |
| Scripts\Godot\HeroStrip.cs | C# | DiceArena.GodotApp | Godot glue — nodes, scene wiring | class HeroStrip |  |
| Scripts\Godot\HoverBubble.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring | class HoverBubble |  |
| Scripts\Godot\IconFlushOnBoot.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class IconFlushOnBoot |  |
| Scripts\Godot\IconLibrary.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring |  | Loads, resizes, and caches icon textures. |
| Scripts\Godot\IconSizeProbe.cs | C# | DiceArena.Godot | Godot glue — nodes, scene wiring | class IconSizeProbe | Editor-only helper (layout/size probing). |
| Scripts\Godot\RollPopup.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring | class RollPopup |  |
| Scripts\Godot\UiScroll.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring |  |  |
| Scripts\Godot\UiUtils.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring |  |  |
| Scripts\Godot\UpgradeChooser.cs | C# | DiceArena.GodotUI | Godot glue — nodes, scene wiring | class UpgradeChooser |  |
| Scripts\Util\Log.cs | C# | DiceArena.Engine.Loadout | Utilities / extensions | class Loadout_Deprecated_IgnoreMe |  |
| Scripts\Util\NodeExtensions.cs | C# | DiceArena.Engine.Loadout | Utilities / extensions |  | C# extension helpers to tidy UI code. |
