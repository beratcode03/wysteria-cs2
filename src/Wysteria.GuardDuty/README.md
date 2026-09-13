# Wysteria.GuardDuty

## Purpose
Records active guard duty times and provides round-based statistics.

## Features
- Allows a guard to officially start and stop their duty session.
- Calculates elapsed time via DateTime differences to eliminate overhead.
- Prints a summary of duty time.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_duty | CT | Toggles guard duty on or off. |
| css_duty_stats | CT | Shows your current duty statistics. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| CT Team | Guards | Only guards can use the duty tracker. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Duty started | Self | Wysteria Default |
| Duty stopped | Self | Wysteria Default |
| Duty stats | Self | Wysteria Default |

## Round Lifecycle
### Round Start
Clears all active session times.

### Round End
Finalizes and summarizes all active sessions for the round.

### Player Spawn
None.

### Player Death
None.

### Disconnect
Removes the disconnecting player's session data to prevent memory leaks.

## UI
None. The plugin is command/event driven.

## Sounds
No custom sound assets are included.

## Configuration
| Setting | Default | Description |
|---|---|---|
| Enabled | true | Toggles the plugin. |

## Storage
- Memory.
- No persistent storage.

## Performance
- Event-driven.
- Uses DateTime.UtcNow instead of repeating 1s timers for extreme efficiency.

## Dependencies
- Wysteria.Core

## Permissions & Security
Restricted to CounterTerrorist team only.
