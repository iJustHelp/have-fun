# Task 04: Wire Web Startup

## Goal

Connect configuration, services, and Blazor startup in `HaveFun.Web`.

## Work

- Configure Blazor with `InteractiveServer` render mode.
- Register master settings binding and validation.
- Register the sentence loading service.
- Register the LAN URL service from Task 05 when available.
- Keep startup free of database, authentication, player routing, and game-round behavior.

## Done Criteria

- `HaveFun.Web` starts with all Stage 1 services registered.
- Startup validates required settings and sentence data.
- The web host remains simple and ready for Stage 2 name entry.

## Verification

- Build the solution from `src`.
- Run `HaveFun.Web`.
- Confirm startup succeeds with valid configuration and sentence data.
