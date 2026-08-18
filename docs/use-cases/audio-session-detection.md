# Automatic detection of new audio sessions

**Category**: MVP

## User story
As a user, I want the app to detect when a new app starts producing audio, so that a routing rule can be applied without me doing anything.

## Preconditions
WaveRouter is running in the background ([[background-tray-execution]]); the Windows Core Audio API is accessible.

## Main flow
1. On startup, WaveRouter subscribes to Windows audio session notifications on all render devices (not only the default one).
2. A new app starts and begins producing audio → a new audio session is created.
3. The watcher receives the notification and identifies the owning process (executable name/path).
4. The event is forwarded to the rule matching/routing service ([[automatic-routing-enforcement]]).

## Alternate / error flows
- Subscription fails at startup (COM error) → log/notify, retry, no silent crash.
- A session is created and destroyed almost instantly (e.g. a brief system sound) → no repeated routing attempts; verify the session is still active before acting.

## Edge cases
- A session has no identifiable process (system sounds) → ignored, no rule can apply.
- The same executable launched multiple times (two instances) → each new session evaluated independently.
- WaveRouter starts after other apps are already producing audio → already-active sessions must also be evaluated at startup, not only future new ones.

## Acceptance criteria
- Given WaveRouter is running in the tray, When a new app starts producing audio, Then a new-session event is raised within a reasonable delay (<2s) with the correct executable name.
- Given a transient system sound plays and stops immediately, When WaveRouter processes it, Then no routing attempt fails visibly to the user.
- Given WaveRouter starts while other apps are already producing audio, When it starts, Then it also evaluates already-active sessions against the current rules.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
