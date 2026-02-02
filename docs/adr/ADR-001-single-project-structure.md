# ADR-001: Single Project Structure

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Solution structure decision

---

## Context

We need to decide how to structure the .NET solution for the Workshop Lottery tool. Options range from a single project to multiple projects with layer separation.

## Decision Drivers

- Application scope is well-defined and focused (CLI tool)
- Team size: individual development
- Expected maintenance: occasional updates
- Testability requirements: unit tests needed
- KISS principle: avoid unnecessary complexity

## Options Considered

### Option 1: Single Console Project with Folder Separation
- One `WorkshopLottery.csproj` with `Models/`, `Services/`, `Infrastructure/` folders
- Separate test project

### Option 2: Multi-Project Clean Architecture
- `WorkshopLottery.Domain` - entities, interfaces
- `WorkshopLottery.Application` - use cases
- `WorkshopLottery.Infrastructure` - Excel I/O
- `WorkshopLottery.Console` - CLI entry point
- `WorkshopLottery.Tests`

### Option 3: Single Project, No Structure  
- Flat file structure in single project

## Decision

**Option 1: Single Console Project with Folder Separation**

## Rationale

1. **Appropriate for scope:** A CLI tool with ~10-15 classes doesn't warrant the overhead of multiple projects
2. **Folder separation provides clarity:** Logical grouping without assembly boundaries
3. **Easy navigation:** Single csproj means simpler build, simpler IDE experience
4. **Testability preserved:** Services use interfaces, enabling mocking; test project remains separate
5. **Future path:** Can extract to multiple projects later if scope grows significantly

Multi-project Clean Architecture (Option 2) would add:
- 4 additional .csproj files
- Cross-project reference management
- More ceremony for minimal benefit in this context

## Consequences

### Positive
- Faster development
- Simpler mental model
- Single namespace to reason about
- Easier onboarding

### Negative
- No compile-time enforcement of layer boundaries
- All classes in same assembly (mitigated by `internal` access modifiers)

### Mitigations
- Use `internal` for implementation classes
- Code review to enforce layer discipline
- Clear folder naming conventions

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Overall architecture specification
