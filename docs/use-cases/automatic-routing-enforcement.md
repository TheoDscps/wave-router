# Automatic routing enforcement (matching + applying)

**Category**: MVP

## User story
As a user, I want the app matching a rule to be automatically switched to the correct Wave Link track, so that I never have to do it manually in Wave Link.

## Preconditions
At least one rule exists ([[rule-management]]); a new or existing audio session ([[audio-session-detection]]) matches a rule's executable.

## Main flow
1. On a new-session event (or startup scan), the routing service looks up a matching rule by executable name.
2. If a match is found, the service applies the per-app audio output override (`IPolicyConfig`) to route that session's output to the mapped Wave Link track.
3. Routing success is confirmed.
4. A notification may confirm the routing ([[routing-notifications]]).

## Alternate / error flows
- No matching rule for the detected app → no action taken, session stays on its current output.
- Matching rule found but the target track no longer exists in Wave Link (renamed/removed) → routing fails, user notified, rule flagged as potentially stale.
- Underlying routing API call fails (permission denied, COM error, elevation required) → caught, logged, user notified, no crash.
- The session ends before routing completes (race condition) → abort silently, no error shown.

## Edge cases
- The user had already manually routed the app elsewhere before WaveRouter acted → WaveRouter still enforces the rule (intended behavior for MVP); a "manual override wins" mode is a possible future evolution, not in MVP.
- Ambiguous process identification (multiple possible matches) → resolved by exact executable name match; ambiguity is logged if detected.
- Elevation required but not granted → routing fails with a clear reason shown to the user (never a silent no-op).

## Acceptance criteria
- Given a rule "obs64.exe → Track 4" exists, When OBS starts and produces audio, Then its output is automatically switched to Track 4 with no manual action.
- Given no rule exists for a newly detected app, When the session starts, Then no routing action is taken and no error is shown.
- Given a rule's target track no longer exists in Wave Link, When the rule is applied, Then routing fails gracefully, the user is notified, and the rule is flagged for review.
- Given the routing call requires elevated permissions not currently granted, When routing is attempted, Then the user receives a clear message explaining the failure.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project). This use case is the technical core flagged as a risk in `CLAUDE.md` (undocumented `IPolicyConfig` API).
