# Speed Control Flags for Spectacular Mode

**Date:** 2026-02-02  
**Type:** Feature Enhancement + Bug Fix  
**Status:** Completed

## Original Request

> I may want to run it slower even... to add more emphasis... can you add a flag like --slow --slower or --slowest?

## Bug Found

Using `--slow`, `--slower`, or `--slowest` as aliases for a `--speed` option caused:
```
Required argument missing for option: '--slowest'.
```

This is because System.CommandLine treats aliases as needing the same value parameter as the main option.

## Solution

Changed from option aliases to **separate boolean flags**:
- `--slow` - Boolean flag (2x slower)
- `--slower` - Boolean flag (3x slower)  
- `--slowest` - Boolean flag (5x slower)

Also needed to use `InvocationContext` pattern in `SetHandler` because System.CommandLine only supports up to 8 typed parameters (we have 9 options).

## Additional Enhancement: Flashy Intro Animation

User also requested: "how could we make some cool animation at the beginning? a reveal of the name or something super flashy?"

Implemented multi-phase intro:
1. **Boot Sequence** - Spinner with fun init messages ("⚡ Powering up quantum randomizer...")
2. **Progress Bar** - "Charging lottery engine" with milestone updates  
3. **Letter Reveal** - FIGlet text reveals character-by-character with garbled→clear animation
4. **Color Heat Effect** - Colors transition: grey → red → orange → yellow → gold
5. **Flash Effect** - Brief screen clear for visual punctuation
6. **Final Reveal** - Full banner with ✨ sparkle borders

## Actions Taken

1. Changed `--slow`/`--slower`/`--slowest` from aliases to separate boolean options
2. Used `InvocationContext` pattern to handle 9+ CLI options
3. Implemented multi-phase flashy intro in `SpectacularRenderer.ShowBannerAsync()`
4. Added `BootMessages` array for init phase variety
5. Updated plan-006.md with bug fix note and flashy intro details
6. Updated README.md with correct CLI documentation

## Usage

```bash
# Just slow (2x)
dotnet run --project src/WorkshopLottery -- -i "input/sample.xlsx" -s 42 --slow

# Maximum drama (5x)
dotnet run --project src/WorkshopLottery -- -i "input/sample.xlsx" -s 42 --slowest

# Speed flags auto-enable spectacular mode, so -S is optional
```
