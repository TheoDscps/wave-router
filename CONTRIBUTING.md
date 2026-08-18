# Contributing to WaveRouter

Thanks for considering a contribution. This is a small solo hobby project, so keep expectations
proportional, but PRs and issues are genuinely welcome.

## Getting set up

- Windows 10/11, [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- `dotnet build WaveRouter.slnx` to build everything
- `dotnet run --project src/WaveRouter` to run the app
- `dotnet test WaveRouter.slnx` to run the test suite (`WaveRouter.Core` only, the rest depends on
  live Windows/COM state, see [Project structure](#project-structure))

## Project structure

```
src/
  WaveRouter.Core/            # pure domain, zero Windows dependency, unit-testable
  WaveRouter.Infrastructure/  # NAudio, Windows/WinRT interop, JSON persistence
  WaveRouter/                 # WPF app, tray icon, composition root
tests/
  WaveRouter.Core.Tests/      # xUnit, covers WaveRouter.Core only
```

`WaveRouter.Infrastructure` and the WPF layer touch real Windows audio APIs and undocumented WinRT
interop (see the code comments in `WaveRouter.Infrastructure/Audio/PolicyConfig/`). Changes there
need to be smoke-tested by actually running the app and exercising the routing, not just a clean build.

## Making a change

1. Fork the repo, branch off `main`.
2. Keep the change scoped: one PR, one concern. Bug fixes shouldn't carry drive-by refactors.
3. `dotnet build WaveRouter.slnx` and `dotnet test WaveRouter.slnx` locally before opening the PR.
4. Open a PR against `main`. CI builds and runs the test suite automatically, so check that it's green.
5. Squash-merge is the default once approved, so commit message hygiene on the branch itself isn't
   critical, but a clear PR title/description is.

## Code style

- Standard .NET naming: `PascalCase` public members, `_camelCase` private fields, `IPascalCase`
  interfaces.
- Comments explain *why*, not *what*: skip comments that just restate the code.
- MVVM on the WPF side, hand-rolled (`WaveRouter.Mvvm`), no MVVM Toolkit or similar dependency.
- Prefer implementing something yourself over adding a new dependency, unless it would be
  significantly long or complex to write.

## Reporting bugs

Open an issue with: what you did, what you expected, what happened instead, and your Wave Link
version if it's routing-related. Wave Link's config format is undocumented and has changed at least
once (see `WaveLinkPaths`/`WaveLinkMixerConfigReader`), so version matters.
