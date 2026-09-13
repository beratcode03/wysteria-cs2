# Wysteria.WardenTimer

## Purpose
Allows the Warden to set up low-frequency event-driven timers for activities.

## Features
- Start a public countdown timer for a specific activity (e.g., Hide and Seek).
- Stop the timer prematurely if needed.
- Uses a single callback, avoiding chat spam.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_wtimer | Warden/Admin | Sets a timer for the Warden (Usage: !wtimer <sec> [label]). |
| css_wtimer_stop | Warden/Admin | Stops the active timer. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| Warden | Warden | Can start and stop timers. |
| @css/admin | Admins | Can override and manage timers. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Timer started | Everyone | Wysteria Default |
| Timer stopped | Everyone | Wysteria Default |
| Timer finished | Everyone | Wysteria Default |

## Round Lifecycle
### Round Start
Clears any active timer state.

### Round End
Clears any active timer state.

### Player Spawn
None.

### Player Death
None.

### Disconnect
None.

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
- Uses a single timer instance. Does not spam chat every second.

## Dependencies
- Wysteria.Core

## Permissions & Security
Validates Warden status via WardenService.
