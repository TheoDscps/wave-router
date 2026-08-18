# WaveRouter — Style Guide

Quick reference for the design tokens defined in `src/WaveRouter/Themes/`. For rationale and the full Visual DNA, see the `## Visual Design Tokens` section in `CLAUDE.md`. To see it live: run the app, tray menu → "Style guide" (`StyleGuideWindow`).

Tone: **premium technical minimalism** — sober, precise, "discreet studio". Grounded in Elgato Wave Link's real branding (dark charcoal + cyan/electric-blue accent) for visual coherence with the software this app integrates with.

## Palette

| Token | Dark | Light |
|---|---|---|
| `BgBase` | `#121316` | `#F7F8FA` |
| `BgSurface` | `#1B1D21` | `#FFFFFF` |
| `BgSurfaceElevated` | `#22242A` | `#F0F1F4` |
| `BorderSubtle` | `#2E3038` | `#E2E4E9` |
| `TextPrimary` | `#F5F6F8` | `#15171B` |
| `TextSecondary` | `#9BA0AC` | `#5B6070` |
| `Accent` | `#3ECFEB` | `#0EA5C4` |
| `AccentHover` | `#5BDBF3` | `#0C8CA8` |
| `Success` | `#3DDC97` | `#1E9E6D` |
| `Warning` | `#F2B84B` | `#B4790F` |
| `Error` | `#F0554D` | `#C9352C` |
| `Info` | `#3ECFEB` | `#0EA5C4` |

**Rule**: never use `Accent` as a text color in light mode (contrast fails AA) — icons/fills/borders only. Use `TextPrimary`/`TextSecondary` for all text.

## Typography

| Role | Font | Size | Weight |
|---|---|---|---|
| Heading | Segoe UI Variable | `FontSizeXl` (20) | SemiBold |
| Body | Segoe UI Variable | `FontSizeBase` (14) | Regular |
| Secondary/caption | Segoe UI Variable | `FontSizeSm` (13) | Regular |
| Mono (paths, exe names) | Cascadia Code | `FontSizeSm` (13) | Regular |

Full scale: `FontSizeXs=11, FontSizeSm=13, FontSizeBase=14, FontSizeLg=16, FontSizeXl=20, FontSize2xl=24, FontSize3xl=32`.

## Spacing

4px grid: `Space1=4, Space2=8, Space3=12, Space4=16, Space6=24, Space8=32, Space12=48, Space16=64`.

## Shapes

`RadiusSm=4` (inputs, badges) · `RadiusMd=8` (cards, panels, buttons) · `RadiusLg=12` (window chrome, dialogs).

## Shadows

`ShadowSm` · `ShadowMd` · `ShadowLg` — increasing blur/depth/opacity, always subtle, never hard-edged.

## Motion

`DurationFast=150ms` · `DurationBase=250ms` · `DurationSlow=400ms` (waveform pulse). Easing: `EasingStandard` (CubicEase, EaseInOut). Respect the OS "reduce animations" setting.

## Component variants (see `StyleGuideWindow.xaml` for live rendering)

- **Button**: default (`BgSurfaceElevated` + border) / `AccentButton` (filled accent, for primary actions) / disabled (opacity 0.4)
- **TextBox**: `BgSurface` background, `BorderSubtle` border, `Accent` border on focus
- **Card** (`Style="{StaticResource Card}"`): `BgSurface` + `BorderSubtle` + `RadiusMd` + `ShadowSm` — base container for rule rows/panels
- All interactive controls: `Cursor="Hand"`, visible 2px accent focus ring (`FocusRing` style) for keyboard navigation

## File map

```
src/WaveRouter/Themes/
  Colors.Dark.xaml       # dark theme color tokens
  Colors.Light.xaml      # light theme color tokens
  Typography.xaml
  Spacing.xaml
  Shapes.xaml
  Shadows.xaml
  Motion.xaml
  Styles.xaml            # base control styles built on the tokens (self-merges the 5 files above)
  Generic.xaml           # merges Typography/Spacing/Shapes/Shadows/Motion/Styles for App.xaml
  ThemeManager.cs         # swaps Colors.Dark.xaml <-> Colors.Light.xaml at runtime
```
