# QOL Framework

Framework extensível para plugins **LabAPI** em SCP: Secret Laboratory. Usado pelo plugin **QOL - Quality of Life**.

## Estrutura

| Área | Descrição |
|------|-----------|
| **Core** | `QOLFrameworkLoader`, `ModuleManager`, `IModule` / `ModuleBase` — ciclo de vida do plugin e módulos |
| **Events** | `QOLEventBus` — subscribe/publish com prioridade; eventos de roles, módulos, jogador (Joined/Left, Damaged, Died), ItemUsed, RoundStarting |
| **CustomRoles** | `CustomRoleManager`, `CustomRole`, `RoleAbility` — roles custom com assign, tick, spawn message |
| **CustomSCPs** | `CustomScpManager`, `CustomScp`, `ScpAbility` — SCPs custom (baseados em roles) |
| **Tweaks** | `TweakManager`, `ITweak` — alterações a parâmetros (ex.: SCP-106 slow, SCP-173 health) |
| **GUI** | `GuiManager`, `GuiScreen`, `GuiLabel`, `GuiProgressBar` — hints e elementos por jogador |
| **Models** | `PrimitiveModel`, `PrimitiveShape`, `PrimitiveModelSpawner` — modelos 3D com AdminToys (LabExtended) |
| **CustomItems** | `CustomItemManager` — triggers de pickup no mundo |
| **Config** | `ModuleConfig`, `ConfigManager` — config base e load/save JSON por módulo |
| **Permissions** | `PermissionHelper`, extensão `Player.HasQOLPermission(string)` — verificação de permissões (integrável com LabAPI/EXILED) |
| **Hybrid** | `ExiledBridge` — CASSIE/Broadcast quando EXILED está presente |
| **Extensions** | `PlayerExtensions`, `StringExtensions` — helpers para jogadores e strings |

## Ciclo de vida dos módulos

- **OnEnabled** / **OnDisabled** — ativar/desativar
- **OnPlayerJoined(Player)** / **OnPlayerLeft(Player)** — jogador entra/sai
- **OnRoundStarted** / **OnRoundEnded** / **OnWaitingForPlayers** — round

Registar módulos antes de `Framework.Initialize()`. O `ModuleManager` encaminha os eventos do LabAPI para cada módulo ativo.

## Eventos (QOLEventBus)

- `Subscribe<T>(Action<T> handler, int priority = 0)` / `Unsubscribe<T>` / `Publish<T>`
- Tipos úteis: `PlayerJoinedEvent`, `PlayerLeftEvent`, `PlayerDamagedEvent`, `PlayerDiedEvent`, `ItemUsedEvent`, `RoundStartingEvent`, `CustomRoleAssignedEvent`, etc.
- Definir `IsCancelled = true` em eventos que o suportem para cancelar a ação (conforme o publicador).

## Config opcional

```csharp
ConfigManager.ConfigDirectory = "C:\\...\\LabAPI\\plugins\\QOL";
var config = ConfigManager.LoadOrCreate<MyConfig>("MeuModulo");
ConfigManager.Save("MeuModulo", config);
```

## Permissões

Por defeito todos têm permissão. Para integrar com LabAPI/LabExtended:

```csharp
PermissionHelper.CheckPermission = (player, permission) => /* ex.: ExPlayer.Get(player).HasPermission(permission) */;
// Depois, nos comandos:
if (!player.HasQOLPermission("qol.comando")) return;
```

## Dependências

- LabAPI, 0LabExtended (opcional para modelos 3D), Assembly-CSharp (jogo), Mirror, Newtonsoft.Json (config).
