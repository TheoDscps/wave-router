# ROADMAP — WaveRouter

Living document — update after each completed task or scope change.

## MVP

- [x] Automatic detection of new audio sessions (new app producing sound) — [use case](docs/use-cases/audio-session-detection.md)
- [x] Rule engine: `executable → Wave Link track` matching — [use case](docs/use-cases/automatic-routing-enforcement.md)
- [x] Automatic routing enforcement (switch the app's audio output to the matched track) — [use case](docs/use-cases/automatic-routing-enforcement.md)
- [x] Simple UI to manage rules (add / edit / delete an app ↔ track association) — [use case](docs/use-cases/rule-management.md)
- [x] Local rule persistence (JSON file) — [use case](docs/use-cases/rule-persistence.md)
- [x] Background execution via system tray icon — [use case](docs/use-cases/background-tray-execution.md)

## v1

- [ ] Auto-start with Windows — [use case](docs/use-cases/windows-autostart.md)
- [x] Dynamic reading of Wave Link's available tracks (via device enumeration, not Wave Link's config file — see CLAUDE.md) — [use case](docs/use-cases/read-wave-link-tracks.md)
- [x] Popup prompting the user to route an app the first time it's seen unconfigured, with a persisted "ignore this app" choice — not originally in the use case docs, added based on user feedback; covers most of what routing-notifications.md's "notify on routing" describes too
- [ ] Discreet notifications when an automatic routing happens (the settings on/off toggle specifically — success/failure balloons already exist) — [use case](docs/use-cases/routing-notifications.md)
- [ ] Pattern-based rules (partial process name, wildcard) instead of exact match only
- [ ] Routing history/log

## v2

- [ ] Rule profiles (e.g. different rule sets for streaming vs. solo recording)
- [ ] Import/export rules

## Out of scope

- Support for other mixing software (OBS, Voicemeeter, etc.)
- Cross-platform support (Mac/Linux) — Windows only
- Cloud sync of rules
- Microphone/input routing
