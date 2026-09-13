# Wysteria.PrisonEvidence

## Purpose
Logs important punishment and kill events into a low-cost memory RAM log for admins.

## Features
- Automatically records player kills and round states.
- Acts as a lightweight auditing trail for Admins without DB overhead.
- Outputs logs directly to the admin's console.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_evidence | @css/admin | Dumps recent evidence logs to console. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| @css/admin | Admins | Can view the evidence log. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Dump notification | Self | Wysteria Default |

## Round Lifecycle
### Round Start
Logs "Round Started" event.

### Round End
None.

### Player Spawn
None.

### Player Death
Logs the attacker and victim.

### Disconnect
None.

## UI
None. Outputs to console.

## Sounds
No custom sound assets are included.

## Configuration
| Setting | Default | Description |
|---|---|---|
| MaxLogLines | 100 | The maximum number of events retained in memory. |

## Storage
- Memory (Ring Buffer).
- No SQL or file IO overhead.

## Performance
- Event-driven.
- Zero disk IO. Highly performant.

## Dependencies
- Wysteria.Core

## Permissions & Security
Console dump is strictly protected by @css/admin.
