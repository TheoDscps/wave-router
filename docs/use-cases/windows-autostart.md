# Auto-start with Windows

**Category**: Nice-to-have

## User story
As a user, I want WaveRouter to start automatically when Windows starts, so that routing works without me remembering to launch it manually.

## Preconditions
WaveRouter is installed.

## Main flow
1. The user enables "Start with Windows" in settings/tray menu.
2. The app registers itself to start at Windows logon (registry Run key or a scheduled task).
3. On the next Windows startup, WaveRouter launches automatically, minimized to tray ([[background-tray-execution]]).

## Alternate / error flows
- Registration fails (permission issue) → user notified, the setting reverts to disabled.
- The user disables the setting → the app unregisters itself from startup.

## Edge cases
- The setting is enabled but the app is later moved/uninstalled → a stale startup entry points to a missing exe; Windows handles this without crashing, but it should ideally be cleaned up on uninstall if an installer exists later.

## Acceptance criteria
- Given "Start with Windows" is enabled, When Windows restarts and the user logs in, Then WaveRouter launches automatically minimized to tray.
- Given "Start with Windows" is disabled by the user, When Windows restarts, Then WaveRouter does not launch automatically.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
