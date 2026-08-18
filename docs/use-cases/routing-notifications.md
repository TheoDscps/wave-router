# Notifications on automatic routing

**Category**: Nice-to-have

## User story
As a user, I want a discreet notification when an app is automatically routed (or when routing fails), so that I know the system is working correctly without having to check manually.

## Preconditions
At least one rule triggered a routing attempt ([[automatic-routing-enforcement]]).

## Main flow
1. The routing service successfully applies a rule.
2. A toast/tray notification appears (e.g. "Discord routed to Track 2").
3. The notification disappears automatically after a few seconds.

## Alternate / error flows
- Routing fails → a distinct notification/style is shown (e.g. "Failed to route Discord: track not found") so the user immediately knows something needs attention.
- Notifications disabled by the user → no toast shown, but the action still happens (silent mode).

## Edge cases
- Many apps starting near-simultaneously (e.g. right after a Windows boot with several startup apps) → multiple notifications in quick succession; batching within a short time window is a possible future polish, not a strict MVP requirement.

## Acceptance criteria
- Given a rule successfully routes an app, When routing succeeds, Then a notification confirms the action within a few seconds.
- Given a rule fails to route an app, When routing fails, Then a distinct failure notification explains the reason.
- Given notifications are disabled in settings, When routing happens (success or failure), Then no toast is shown but the routing/logging still occurs.

## Cross-references
None (no `schema.dbml` or `docs/api/` in this project).
