# Dynamic reading of Wave Link tracks

**Category**: Nice-to-have

**Implementation note**: built via Windows device enumeration (`WaveLinkTrackProvider`), not by reading Wave Link's config file as originally planned below — Wave Link registers each track as a real Windows playback device, which turned out to be simpler and avoids depending on an undocumented file format. The user-facing behavior (main flow, alternate/error flows, acceptance criteria) is unchanged; see CLAUDE.md's "Resolved decisions" for the technical writeup.

## User story
As a user, I want the app to automatically list the available Wave Link tracks, so that I can create routing rules without typing track names manually.

## Preconditions
Wave Link is installed and configured with at least one track (Local Input).

## Main flow
1. The user opens the rule management UI.
2. The app reads Wave Link's local configuration file to enumerate available tracks.
3. The app displays the list in a selector.

## Alternate / error flows
- Wave Link not installed / config file not found → show an info message, fall back to manual track name entry.
- Config file unreadable (different Wave Link version, format changed) → show a warning, fall back to manual entry.
- Wave Link config changes while WaveRouter is running (track renamed/added) → re-read on next UI open, or provide a refresh button.

## Edge cases
- Zero tracks configured in Wave Link.
- A track is renamed after a rule already referenced it (stale rule).

## Acceptance criteria
- Given Wave Link is installed with 3 configured tracks, When the user opens the rule creation UI, Then the 3 tracks appear in the track selector.
- Given the Wave Link config file cannot be found, When the user opens the UI, Then a manual text entry field is shown instead of the selector.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
