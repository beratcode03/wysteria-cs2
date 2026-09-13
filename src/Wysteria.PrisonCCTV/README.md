# Wysteria.PrisonCCTV

## Purpose
Allows admins and guards to monitor predefined CCTV camera points on the map.

## Features
- Teleports the player's view to a configured camera point.
- Renders the player invulnerable and sets their view angles.
- Supports restoring the player's original location and state upon exit.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_cctv | @css/admin or CT | Open CCTV camera view. |
| css_cctv_exit | None | Exit CCTV camera view. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| @css/admin | Admins | Can access CCTV regardless of team. |
| CT Team | Guards | Can access CCTV naturally. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Camera selected | Self | Wysteria Default |
| Exit camera | Self | Wysteria Default |
| No cameras | Self | Wysteria Default |

## Round Lifecycle
### Round Start
Clears saved player positions.

### Round End
None.

### Player Spawn
None.

### Player Death
Clears the dead player's saved position.

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
| Cameras | [] | List of camera points (X, Y, Z, Pitch, Yaw, Roll). |

## Storage
- Memory.
- No persistent storage.

## Performance
- Event-driven.
- No timers or per-tick logic.

## Dependencies
- Wysteria.Core

## Permissions & Security
Relies on AdminManager and TeamNum validation.
