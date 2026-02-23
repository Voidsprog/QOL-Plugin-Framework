# QOL Framework

**Extensible plugin framework for SCP: Secret Laboratory** — built for **LabAPI**, used by the **QOL (Quality of Life)** plugin.  
Modular architecture for custom roles, SCPs, tweaks, GUI, events, and a rich set of utilities (v2.0).

---

## Overview

| | |
|---|---|
| **Version** | 2.0.0 |
| **Target** | LabAPI (SCP:SL) |
| **Hybrid** | Optional EXILED bridge (CASSIE, broadcast) |
| **Config** | JSON per module, YAML via main plugin |

---

## Feature Summary

### Core

| Component | Description |
|-----------|-------------|
| **QOLFrameworkLoader** | Singleton entry point. Initializes and shuts down modules, roles, tweaks, GUI, and utilities. |
| **ModuleManager** | Register / enable / disable modules. Forwards LabAPI events (round, players) to active modules. |
| **IModule** | Interface: `Name`, `Description`, `Author`, `Version`, `Priority`, `IsEnabled`; lifecycle hooks below. |
| **ModuleBase** | Abstract base with `Log` / `LogWarning` / `LogError`. |
| **ModuleBase&lt;TConfig&gt;** | Module with typed `Config` (extends `ModuleConfig`). |

**Module lifecycle:** `OnEnabled` → `OnDisabled` · `OnPlayerJoined` / `OnPlayerLeft` · `OnRoundStarted` / `OnRoundEnded` / `OnWaitingForPlayers`.

---

### Custom Roles & SCPs

| Component | Description |
|-----------|-------------|
| **CustomRoleManager** | Register roles, `AssignRole(player, id)`, `GetPlayerRole`, `HasCustomRole`, `RemoveFromAllRoles`. |
| **CustomRole** | Abstract role: `Id`, `Name`, `BaseRole`, `Team`, `MaxHealth`, `SpawnInventory`, `CustomInfo`, `Abilities`, `UseSpawnpoint`. |
| **RoleAbility** | Cooldown-based ability: `TryActivate`, `OnTick` / `OnAssigned` / `OnRemoved`. |
| **CustomScpManager** | Manages custom SCP roles (uses CustomRoleManager). |
| **CustomScp** | Extends CustomRole, `Team.SCPs`, `ScpNumber`, `BaseHealth`, `ArtificialHealthMax`, `ScpAbilities`. |
| **ScpAbility** | SCP-specific ability with cooldown. |
| **ScpParameterTweak** | Base tweak for SCP params: `TargetScp`, `HealthOverride`, `AhpOverride`, `WalkSpeedMultiplier`. |

---

### Tweaks

| Component | Description |
|-----------|-------------|
| **TweakManager** | Register tweaks; `Initialize` → Apply, `Shutdown` → Revert. |
| **ITweak** | `Name`, `Description`, `Category`, `IsEnabled`, `Apply()`, `Revert()`. |
| **TweakBase** | Abstract: `OnApply` / `OnRevert`. |

---

### GUI

| Component | Description |
|-----------|-------------|
| **GuiManager** | Per-player screens, `RefreshRate`, `GetOrCreateScreen`, `ShowElement`, `HideElement`. |
| **GuiScreen** | Screen with ordered elements. |
| **GuiElement** | Abstract element base. |
| **GuiLabel** | Formatted label. |
| **GuiProgressBar** | Progress bar with percentage. |
| **GuiMenu** | Menu with selection and items. |

---

### Models & Custom Items

| Component | Description |
|-----------|-------------|
| **PrimitiveModel** | 3D model as a list of `PrimitiveShape` (type, position, rotation, scale, color). |
| **PrimitiveShape** | Single primitive definition. |
| **PrimitiveModelSpawner** | Spawns models via PrimitiveToy (AdminToys / LabExtended). |
| **CustomItemManager** | Spawn invisible triggers in the world; callback when a player enters radius. `SpawnTrigger`, `DestroyAll`. |

---

### Events

| Component | Description |
|-----------|-------------|
| **QOLEventBus** | Pub/sub with priority: `Subscribe<T>(handler, priority)`, `Unsubscribe<T>`, `Publish<T>`. |
| **QOLEventArgs** | Base with `IsCancelled`. |
| **Built-in events** | `PlayerJoinedEvent`, `PlayerLeftEvent`, `PlayerDamagedEvent`, `PlayerDiedEvent`, `ItemUsedEvent`, `RoundStartingEvent`, `CustomRoleAssignedEvent`, `CustomRoleRemovedEvent`, `TweakAppliedEvent`, `ModuleEnabledEvent`, `ModuleDisabledEvent`. |

---

### Extensions

| Area | Description |
|------|-------------|
| **PlayerExtensions** | `HasQOLPermission`, `SendQOLHint`, `SendColoredHint`, `IsScp`, `IsHuman`, `IsAlive`, `GetClosestPlayer`, `GetNearbyPlayers`, `GetDistanceTo`, `HealToMax`, `HealPercent`, `DamagePercent`, `BroadcastToAll`. |
| **StringExtensions** | `Color`, `Bold`, `Italic`, `Size`, `Underline`, `StrikeThrough`, `QOLTag`, `ScpTag`, `WarningTag`, `ErrorTag`, `SuccessTag`. |

---

### Config & Permissions

| Component | Description |
|-----------|-------------|
| **ModuleConfig** | Base with `IsEnabled`. |
| **ConfigManager** | `ConfigDirectory`, `GetConfigPath`, `Load<T>`, `Save<T>`, `LoadOrCreate<T>` (JSON per module). |
| **PermissionHelper** | Configurable `CheckPermission` delegate; `HasPermission(player, permission)`. Default: allow all. |

---

### Hybrid (EXILED)

| Component | Description |
|-----------|-------------|
| **ExiledBridge** | Detects EXILED at runtime. `TryCassieMessage`, `TryBroadcast` when EXILED is present. |

---

### Utilities (v2.0)

| Utility | Description |
|---------|-------------|
| **MapUtilities** | `AllRooms`, `GetRoomsByZone`, `GetLczRooms` / `GetHczRooms` / `GetEzRooms` / `GetSurfaceRooms`, `GetRoomName`, `FindRoomsByName`, `GetRandomRoom`, `GetClosestRoom`, `GetZoneAt`, `GetSpawnablePosition`, `GetPlayersInZone` / `GetPlayersInRoom` / `GetPlayersInRadius`, `DistanceBetweenRooms`. |
| **RoundUtilities** | `IsRoundActive`, `RoundElapsed`, `RoundElapsedSeconds`, `HasElapsed`, `GetAlivePlayerCount` / `GetAliveHumanCount` / `GetAliveScpCount`, `GetAlivePlayers` / `GetAliveHumans` / `GetAliveScps`, `GetPlayersByTeam` / `GetPlayersByRole`, `GetRandomAlivePlayer`, `GetTeamCounts`, `FormatElapsed`. |
| **PlayerDataStore** | Per-player, per-round temp data: `Set<T>`, `Get<T>`, `Has`, `Remove`, `ClearPlayer`, `Increment`, `IncrementFloat`, `Toggle`. Cleared on round start. |
| **CoroutineHelper** | MEC helpers: `CallDelayed`, `CallRepeating`, `CallFor`, `RunNamed`, `KillNamed`, `KillAll`, `IsRunning`, `Sequence(steps)`. |
| **CooldownManager** | Global and per-player cooldowns: `IsOnCooldown`, `SetCooldown`, `TryUse` (run action only if not on cooldown), `GetRemainingCooldown`, `RemoveCooldown`, `ClearPlayer`. |
| **CassieHelper** | CASSIE helpers: `Announce`, `AnnounceDelayed`, `AnnounceSequence`, `ScpEscaped`, `ScpContained`, `ScpTerminated`, `FacilityAlert`, `WarheadCountdown`, `LightsOut`, `CustomAlert`. |
| **DamageModifierSystem** | Damage modifiers (multiplier + flat) per player and global; `AddGlobalModifier`, `AddPlayerModifier`, `CalculateDamage(baseDamage, target)`. |
| **InventoryHelper** | `HasItem`, `CountItem`, `GiveItem`, `GiveItems`, `RemoveItem`, `RemoveAllOfType`, `ClearInventory`, `SetLoadout`, `IsInventoryFull`, `GetItemTypes`, `HasKeycard`, `HasWeapon`, `HasMedical`. |
| **RespawnManager** | `RespawnAs(player, role, delay)`, `RespawnAsWithLoadout`, `RespawnAllDead`, `CancelPendingRespawn`, `HasPendingRespawn`. |
| **BroadcastManager** | Queued broadcasts with priority; `Send` / `SendImmediate`, `SendToAll` / `SendImmediateToAll`, `ClearQueue` / `ClearAllQueues`. Avoids hint overlap. |
| **RandomHelper** | `Chance(percent)`, `Pick<T>`, `PickWeighted<T>`, `Shuffle`, `PickMultiple`, `Range(int|float)`, `RandomOffset`. |

Utilities are initialized/cleaned by the loader; round-scoped data is cleared in `OnWaitingForPlayers`.

---

## Usage

### Registering modules (before `Initialize()`)

```csharp
var framework = new QOLFrameworkLoader();
framework.Modules.Register(new MyModule());
framework.Roles.RegisterRole(new MyCustomRole());
framework.Tweaks.Register(new MyTweak());
framework.Initialize();
```

### Event bus

```csharp
QOLEventBus.Subscribe<PlayerDamagedEvent>(ev => {
    ev.Amount *= 0.5f;
}, priority: 10);
QOLEventBus.Publish(new PlayerDamagedEvent { Target = p, Amount = 50f });
```

### Optional config (JSON per module)

```csharp
ConfigManager.ConfigDirectory = @"C:\...\LabAPI\plugins\QOL";
var config = ConfigManager.LoadOrCreate<MyConfig>("MyModule");
ConfigManager.Save("MyModule", config);
```

### Permissions (plug in your backend)

```csharp
PermissionHelper.CheckPermission = (player, permission) =>
    ExPlayer.Get(player)?.HasPermission(permission) ?? false;
// In commands:
if (!player.HasQOLPermission("qol.command")) return;
```

---

## Dependencies

- **LabAPI** — SCP:SL plugin API  
- **0LabExtended** — optional (AdminToys / primitive models)  
- **Assembly-CSharp** (game), **Mirror**, **Newtonsoft.Json**  
- **MEC** (coroutines) — from game/LabAPI

---

## License

See repository license. QOL Framework is part of the QOL plugin project for SCP: Secret Laboratory.
