# Background execution via system tray

**Category**: MVP

## User story
As a user, I want WaveRouter to run in the background with a tray icon, so that it can keep watching and routing without a visible window cluttering my desktop.

## Preconditions
WaveRouter is installed/launched.

## Main flow
1. The user launches WaveRouter.
2. The main window (rule management UI) may show on first launch, or the app starts minimized to tray depending on settings.
3. A tray icon appears in the Windows notification area.
4. The user closes the main window; the app keeps running in the background (watcher active), tray icon remains.
5. Right-click on the tray icon shows a context menu (Open rules, Pause routing, Exit).
6. Left-click/double-click opens the main window.

## Alternate / error flows
- The user clicks "Exit" in the tray menu → the app fully closes, the watcher stops, no more auto-routing until relaunched.
- The watcher crashes/stops unexpectedly while the tray icon remains → the icon must reflect a "not working" state instead of silently doing nothing.

## Edge cases
- The user accidentally launches a second instance (double-click the exe twice) → the second instance detects the first is running and either focuses it or exits, instead of running two watchers in parallel (which would double-apply routing).
- No visible system tray (rare Windows configuration) → the app must still function.

## Acceptance criteria
- Given WaveRouter is launched, When the main window is closed (not exited), Then the app continues running in the tray and continues auto-routing new sessions.
- Given WaveRouter is already running, When the user launches a second instance, Then the second instance focuses the existing window (or exits) instead of starting a second watcher.
- Given the user clicks "Exit" in the tray menu, When confirmed, Then the app fully stops (watcher unsubscribed, process exits).

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
