# Task 01: Scaffold Solution and Projects

## Goal

Create the Stage 1 .NET project foundation under `src`.

## Work

- Create `src`.
- Create solution `src/HaveFun.sln`.
- Create `src/HaveFun.Web` as an ASP.NET Core Blazor Web App.
- Configure `HaveFun.Web` with `InteractiveServer` render mode.
- Create `src/HaveFun.Core` as a class library.
- Add both projects to `HaveFun.sln`.
- Add a project reference from `HaveFun.Web` to `HaveFun.Core`.

## Done Criteria

- `src/HaveFun.sln` exists.
- `HaveFun.Web` and `HaveFun.Core` build as part of the solution.
- `HaveFun.Web` is the runnable web app.
- All implementation code lives under `src`.

## Verification

- Run a solution build from `src`.
- Run `HaveFun.Web` and confirm the default app starts.
