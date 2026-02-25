# QOL Framework — Roadmap

Roadmap de melhorias e tarefas para a **QOL Framework** (e integração com o plugin QOL).  
Atualizado conforme o estado do projeto.

---

## Legenda de prioridade

| Símbolo | Significado        |
|--------|--------------------|
| P0     | Crítico / bloqueador |
| P1     | Alta prioridade    |
| P2     | Média              |
| P3     | Baixa / quando der |

---

## Concluído (v2.0)

- [x] Core: ModuleManager, ModuleBase, lifecycle (round, player join/left)
- [x] Custom roles (Containment Engineer) e CustomScp (SCP-035 em standby)
- [x] Tweaks (SCP-106, SCP-173)
- [x] Utilities: MapUtilities, RoundUtilities, PlayerDataStore, CoroutineHelper, CooldownManager, BroadcastManager, CassieHelper, DamageModifierSystem, InventoryHelper, RespawnManager, RandomHelper
- [x] Extensions: PlayerExtensions (incl. `IsValid`), StringExtensions
- [x] Limpeza ao sair do jogador (PlayerDataStore, CooldownManager, BroadcastManager)
- [x] `RunSafe()` no ModuleBase; `CallDelayed(delay, player, action)` no CoroutineHelper
- [x] ExiledBridge (CASSIE/broadcast quando EXILED está presente)
- [x] README da framework em inglês com resumo de features
- [x] **Curto prazo (1–4):** Null-safety nos utilities; BroadcastManager por prioridade; XML docs; PlayerIdHelper unificado
- [x] **Médio prazo (8–10):** Dependências entre módulos (RequiredModules); hooks OnPlayerHurting/OnPlayerDying + QOLEventBus; Config por módulo JSON (ConfigManager + Blackout exemplo)

---

## Curto prazo (estabilidade e polish)

### Framework (código)

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| ~~1~~ | ~~Garantir null-safety em todos os utilities~~ | P1 | ✅ Feito: MapUtilities, RoundUtilities, BroadcastManager, CassieHelper, RandomHelper, RespawnManager |
| ~~2~~ | ~~BroadcastManager: processar fila por prioridade~~ | P2 | ✅ Feito: fila é List; ordenação por Priority (desc) antes de enviar |
| ~~3~~ | ~~Documentar XML (summary) em APIs públicas~~ | P2 | ✅ Feito: MapUtilities, RoundUtilities, PlayerDataStore, CooldownManager, BroadcastManager, PlayerIdHelper |
| ~~4~~ | ~~Unificar identificação de jogador~~ | P2 | ✅ Feito: `PlayerIdHelper.GetId(Player)` usado em PlayerDataStore, CooldownManager, BroadcastManager, DamageModifierSystem, RespawnManager |

### Plugin QOL

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 5 | Revisar todos os módulos para usar `player.IsValid()` antes de efeitos (CoinFlip já usa) | P1 | ContainmentEngineer, Blackout, etc. |
| 6 | Opção de config para desativar efeitos “perigosos” da moeda (ex.: SwitchRole, SetToZombie, PocketTeleport) | P2 | Reduzir risco de bugs/desync em servidores mais sensíveis |
| 7 | Testes manuais (checklist) por módulo: CoinFlip, Blackout, CE, ScpTweaks | P2 | Documentar num TESTING.md ou na wiki |

---

## Médio prazo (novas capacidades na framework)

### Core e módulos

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| ~~8~~ | ~~Suporte a dependências entre módulos~~ | P2 | ✅ Feito: ModuleBase.RequiredModules; ModuleManager verifica dependências |
| ~~9~~ | ~~Hook OnPlayerDamaged/OnPlayerDied no ModuleManager~~ | P2 | ✅ Feito: Hurting/Dying; OnPlayerHurting/OnPlayerDying; QOLEventBus |
| ~~10~~ | ~~Config por módulo em JSON (ConfigManager)~~ | P2 | ✅ Feito: ConfigDirectory no plugin; LoadModuleConfig/SaveModuleConfig; Blackout JSON |

### Roles e abilities

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 11 | Documentar e exemplificar RoleAbility (cooldown, OnTick, OnAssigned/OnRemoved) | P2 | Para facilitar novas roles com habilidades |
| 12 | Sistema de “tags” ou “flags” por jogador (ex.: HasTag("ce"), AddTag/RemoveTag) além do PlayerDataStore | P3 | Pode ser uma camada em cima de PlayerDataStore com keys reservadas |

### Utilities

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 13 | MapUtilities: obter sala por tipo (RoomType) compatível com LabAPI/MapGeneration | P2 | Alinhar com o que o CameraSystem EXILED usa (Room.Get) |
| 14 | CassieHelper: mais presets (ex.: “All clear”, “Facility lockdown”) e suporte a subtitles | P3 | |
| 15 | InventoryHelper: “GiveItemSafe” que não falha se inventário cheio (devolve bool) | P2 | |

### GUI e eventos

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 16 | Exemplo mínimo de uso do GuiManager (um módulo que mostra um GuiLabel/GuiProgressBar) | P2 | No README ou em QOL como módulo de exemplo |
| 17 | QOLEventBus: garantir que eventos built-in são publicados nos sítios certos (round start/end, damage, etc.) | P1 | Se já estiver feito, documentar no README |

---

## Longo prazo (evolução maior)

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 18 | SCP-035 (The Mask): reativar quando o problema de cliente/DLL estiver resolvido; testes de stress (múltiplos jogadores, troca de role) | P1 | Código em _Standby |
| 19 | Suporte a “reload” de config em runtime (sem reiniciar servidor) para módulos que suportem | P3 | Comando ou evento LabAPI |
| 20 | Framework como pacote NuGet ou DLL estável versionada (sem dependência a paths do SCP:SL) | P3 | Para outros plugins usarem só a framework |
| 21 | Integração opcional com EXILED mais profunda (ex.: traduzir eventos EXILED → QOLEventBus para módulos únicos) | P3 | Manter compatibilidade com servidor só LabAPI |

---

## Plugin QOL (fora da framework)

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 22 | CameraSystem: mantido como plugin EXILED separado; documentar no QOL que requer CE (CustomInfo) | P2 | Já implementado |
| 23 | Comandos admin por módulo (ex.: blackout manual, dar role CE a alguém) | P2 | Opcional |
| 24 | Traduções (i18n) para hints/mensagens dos módulos | P3 | Ficheiros de tradução ou config por idioma |

---

## Manutenção e docs

| # | Tarefa | Prioridade | Notas |
|---|--------|------------|--------|
| 25 | Manter ROADMAP.md atualizado (marcar concluído, adicionar itens novos) | P2 | |
| 26 | CHANGELOG.md por versão da framework (ex.: v2.0.0, v2.1.0) | P3 | |
| 27 | Versão mínima de LabAPI (e de EXILED se aplicável) documentada no README | P2 | |

---

## Como usar este roadmap

- **Sprints**: escolher 2–3 itens de curto prazo por ciclo.
- **Prioridade**: P0/P1 primeiro; P2 quando estável; P3 em tempo restante.
- **Concluído**: mover item para a secção “Concluído” e indicar versão (ex.: v2.1.0).

Se quiseres, no próximo passo podemos transformar isto em issues (por exemplo para GitHub) ou detalhar uma secção concreta (ex.: só utilities ou só roles).
