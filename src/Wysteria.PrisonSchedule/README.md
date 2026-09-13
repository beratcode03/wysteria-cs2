# Wysteria.PrisonSchedule

## Purpose
Manages the jailbreak day schedule activities and broadcasts current events.

## Features
- Defined list of daily prison activities (e.g., Gym, Yard, Diner).
- Warden can advance the schedule to the next state.
- Players can check the current and next schedule.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_schedule | All | View current and next scheduled activity. |
| css_next_activity | Warden/Admin | Advance to the next scheduled activity. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| Warden | Warden | Can advance the schedule. |
| @css/admin | Admins | Can advance the schedule. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Activity started | Everyone | Wysteria Default |
| Schedule concluded | Everyone | Wysteria Default |
| Check schedule | Self | Wysteria Default |

## Round Lifecycle
### Round Start
Resets the schedule index.

### Round End
None.

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
| Activities | Array | List of string activity names. |

## Storage
- Memory.
- No persistent storage.

## Performance
- Event-driven.
- No countdown spam.

## Dependencies
- Wysteria.Core

## Permissions & Security
Warden validation is strictly enforced.
