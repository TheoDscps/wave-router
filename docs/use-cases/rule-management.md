# Rule management (create / edit / delete)

**Category**: MVP

## User story
As a user, I want to create, edit, and delete routing rules (app ↔ track), so that I can control which app goes to which track.

## Preconditions
WaveRouter is running; the track list is available ([[read-wave-link-tracks]]) or manual entry is used as a fallback.

## Main flow
1. The user opens the rule list in the main window.
2. They click "Add rule".
3. They select/enter the target executable.
4. They select the target Wave Link track.
5. They save; the rule appears in the rule list.

## Alternate / error flows
- Duplicate rule for the same executable → warn, ask for confirmation before overwriting.
- Empty executable or track selection on save → block save, show a validation error.
- Executable path/name doesn't exist on disk → warn but allow save (valid case: preparing a rule for an app not yet installed).

## Edge cases
- Two different processes share the same executable name but different paths — which rule applies?
- A very long rule list — the UI must stay usable (scroll/search).
- Editing a rule while a matching audio session is currently active.

## Acceptance criteria
- Given no existing rule for "valorant.exe", When the user creates "valorant.exe → Track 3" and saves, Then the rule appears in the rule list and is persisted.
- Given a rule for "discord.exe" already exists, When the user tries to create a second rule for "discord.exe", Then the app warns about the duplicate and asks for confirmation before overwriting.
- Given an existing rule, When the user deletes it, Then it's removed from the list and from storage, and future sessions of that app are no longer auto-routed.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
