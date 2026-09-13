# Wysteria.PrisonMail

## Purpose
Allows players to send private cell mails to each other with a constrained inbox limit.

## Features
- Direct asynchronous player-to-player messaging.
- Inbox with a maximum capacity.
- Message length limits to prevent spam.

## Commands
| Command | Permission | Description |
|---|---|---|
| css_mail | All | Send a mail to a player (Usage: !mail <name> <msg>). |
| css_inbox | All | Check your inbox. |
| css_mail_clear | All | Clear your inbox. |

## Permissions
| Permission | Who | Description |
|---|---|---|
| None | All | Available to all players. |

## Chat Messages
| Message/Event | Audience | Color/Format |
|---|---|---|
| Mail sent | Self | Wysteria Default |
| Mail received | Target | Wysteria Default |
| Inbox status | Self | Console Output & Chat Notification |

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
None.

## UI
None. Messages are printed to the client console to avoid UI spam.

## Sounds
No custom sound assets are included.

## Configuration
| Setting | Default | Description |
|---|---|---|
| MaxMessageLength | 100 | Maximum characters per mail. |
| MaxInboxSize | 5 | Maximum mails a player can hold. |

## Storage
- Memory.
- Cleared on map change natively.

## Performance
- Event-driven.
- No timers or per-tick logic.

## Dependencies
- Wysteria.Core

## Permissions & Security
Input length is strictly capped.
