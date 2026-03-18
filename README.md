# QOL Framework

Extensible framework for **LabAPI** plugins in SCP: Secret Laboratory. Used by the **QOL - Quality of Life** plugin.

## Structure

| Area | Description |
|------|-------------|
| **Core** | `QOLFrameworkLoader`, `ModuleManager`, `IModule` / `ModuleBase` — plugin and module lifecycle |
| **Events** | `QOLEventBus` — subscribe/publish with priority; events for roles, modules, player (Joined/Left, Damaged, Died), ItemUsed, RoundStarting |
| **CustomRoles** | `CustomRoleManager`, `CustomRole`, `RoleAbility` — custom roles with assign, tick, spawn message |
| **CustomSCPs** | `CustomScpManager`, `CustomScp`, `ScpAbility` — custom SCPs (role-based) |
| **Tweaks** | `TweakManager`, `ITweak` — parameter adjustments (e.g.: SCP-106 slow, SCP-173 health) |
| **GUI** | `GuiManager`, `GuiScreen`, `GuiLabel`, `GuiProgressBar` — hints and per-player UI elements |
| **Models** | `PrimitiveModel`, `PrimitiveShape`, `PrimitiveModelSpawner` — 3D models using AdminToys (LabExtended) |
| **CustomItems** | `CustomItemManager` — world pickup triggers |
| **Config** | `ModuleConfig`, `ConfigManager` — base config and per-module JSON load/save |
| **Permissions** | `PermissionHelper`, extension `Player.HasQOLPermission(string)` — permission checks (integrates with LabAPI/EXILED) |
| **Hybrid** | `ExiledBridge` — CASSIE/Broadcast when EXILED is present |
| **Extensions** | `PlayerExtensions`, `StringExtensions` — helpers for players and strings |

## Module lifecycle

- **OnEnabled** / **OnDisabled** — enable/disable  
- **OnPlayerJoined(Player)** / **OnPlayerLeft(Player)** — player join/leave  
- **OnRoundStarted** / **OnRoundEnded** / **OnWaitingForPlayers** — round events  

Register modules before `Framework.Initialize()`. The `ModuleManager` routes LabAPI events to each active module.

## Events (QOLEventBus)

- `Subscribe<T>(Action<T> handler, int priority = 0)` / `Unsubscribe<T>` / `Publish<T>`
- Useful types: `PlayerJoinedEvent`, `PlayerLeftEvent`, `PlayerDamagedEvent`, `PlayerDiedEvent`, `ItemUsedEvent`, `RoundStartingEvent`, `CustomRoleAssignedEvent`, etc.
- Set `IsCancelled = true` on supported events to cancel the action (depending on the publisher).

## Optional config

```csharp
ConfigManager.ConfigDirectory = "C:\\...\\LabAPI\\plugins\\QOL";
var config = ConfigManager.LoadOrCreate<MyConfig>("MyModule");
ConfigManager.Save("MyModule", config);

Permissions

By default, everyone has permission. To integrate with LabAPI/LabExtended:
PermissionHelper.CheckPermission = (player, permission) => /* e.g.: ExPlayer.Get(player).HasPermission(permission) */;
// Then, in commands:
if (!player.HasQOLPermission("qol.command")) return;

Dependencies

LabAPI, LabExtended (optional for 3D models), Assembly-CSharp (game), Mirror, Newtonsoft.Json (config).
