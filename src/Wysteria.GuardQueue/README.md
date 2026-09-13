# Wysteria.GuardQueue

## Purpose
Manages a priority queue for prisoners waiting to join the CT team.

## Features
- Players can join or leave the queue.
- Prevents team-join spamming by providing a structured waitlist.
- Cleans up disconnected players automatically.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_guard | T | Join the CT queue. |
| css_qleave | All | Leave the CT queue. |
| css_qstatus | All | Check your position in the CT queue. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| None | All | Any T player can queue. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Joined queue | Self | Wysteria Default |
| Left queue | Self | Wysteria Default |
| Queue status | Self | Wysteria Default |

## Round Lifecycle
### Round Start
None.

### Round End
None.

### Player Spawn
None.

### Player Death
None.

### Disconnect
Removes the disconnecting player from the queue.

## UI
None. The plugin is command/event driven.

## Sounds
No custom sound assets are included.

## Configuration
| Setting | Default | Description |
|---|---|---|
| MaxQueueSize | 10 | The maximum number of players allowed in the queue. |

## Storage
- Memory.
- No persistent storage.

## Performance
- Event-driven.
- No per-tick logic.

## Dependencies
- Wysteria.Core

## Permissions & Security
Prevents existing guards from joining the queue.
