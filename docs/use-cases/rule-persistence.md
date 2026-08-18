# Local persistence of rules

**Category**: MVP

## User story
As a user, I want my rules to persist across app restarts, so that I don't have to reconfigure them every time I open WaveRouter.

## Preconditions
WaveRouter has write access to `%AppData%`.

## Main flow
1. The user creates/edits/deletes a rule via the UI ([[rule-management]]).
2. The app serializes the rule set to JSON and writes it to `%AppData%/WaveRouter/rules.json`.
3. On next startup, WaveRouter reads `rules.json` and loads the rule set into memory.

## Alternate / error flows
- `rules.json` missing on startup (first run) → start with an empty rule list, create the file on first save.
- `rules.json` corrupted/invalid JSON → don't crash; back up the corrupted file (`rules.json.bak`), start with an empty rule set, notify the user.
- Write failure (disk full, permissions) → show an error, keep the in-memory state, no silent data loss.

## Edge cases
- Concurrent writes (guarded against by preventing a second app instance, see [[background-tray-execution]]).
- Unclean shutdown (crash) mid-write — writes must be atomic (write to a temp file then rename) to avoid corrupting `rules.json`.

## Acceptance criteria
- Given WaveRouter is closed and reopened, When it starts, Then all previously saved rules are loaded and shown in the UI.
- Given `rules.json` is corrupted, When WaveRouter starts, Then it starts with an empty rule list, backs up the corrupted file, and notifies the user instead of crashing.
- Given a rule is created, When the write to disk fails, Then the user sees an error message and the rule is not silently lost from the UI state.

## Cross-references
None (no `schema.dbml` — rules are stored as a local JSON file by design, not a SQL database).
