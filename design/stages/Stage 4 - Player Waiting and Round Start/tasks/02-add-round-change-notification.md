# Task 02: Add Round Change Notification

## Goal

Let Blazor player pages react when the Game Master starts a round.

## Work

- Extend `IGameStateService` with a round-changed notification.
- Raise the notification when a new round starts.
- Keep the notification in memory and process-local.
- Avoid manual SignalR hubs, polling, databases, or message brokers.
- Ensure subscribers can safely unsubscribe.

## Done Criteria

- Consumers can subscribe to round changes.
- Starting a round notifies subscribers.
- Notification behavior stays inside server memory.
- Existing dashboard start behavior still works.

## Verification

- Build the solution from `src`.
- Confirm a test subscriber or player page can receive the round-start notification.
