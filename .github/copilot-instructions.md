# Workshop Lottery Project - Copilot Instructions

This repository contains the Workshop Lottery System - a .NET 10 console application for fair workshop seat assignment using weighted lottery.

## Model Preference

**Always use Claude Opus 4.5** for this project when available.

## Key Documentation

Before implementing anything, consult these documents:

1. **Architecture**: `docs/ARCHITECTURE.md` - Full system specification
2. **ADRs**: `docs/adr/` - Architecture Decision Records explaining design choices
3. **Implementation Plans**: `docs/plans/` - Phase-by-phase implementation guides
4. **Prompts**: `docs/prompts/` - All requests and their context

## Mandatory: Document All Requests

**Every user request or task must be documented as a prompt file.**

Location: `docs/prompts/NNN-description.md`

This ensures:
- Full audit trail of development decisions
- Context preservation for future reference
- Alignment verification against architecture

## Technology Decisions (from ADRs)

| Decision | Choice | ADR |
|----------|--------|-----|
| Project Structure | Single project with folders | ADR-001 |
| Excel Library | ClosedXML | ADR-002 |
| Lottery Algorithm | Efraimidis-Spirakis | ADR-003 |
| CLI Framework | System.CommandLine | ADR-004 |
| Column Matching | Fuzzy contains-based | ADR-005 |
| Assignment Strategy | Two-wave fairness | ADR-006 |

## Code Conventions

- .NET 10 with C# modern features
- File-scoped namespaces
- Records for immutable data
- Interfaces for all services
- Unit tests with xUnit + FluentAssertions
- Nullable reference types enabled

## Quick Context

The app:
1. Reads MS Forms Excel exports with workshop registrations
2. Validates eligibility (laptop, commitment, no duplicates)
3. Runs weighted lottery using Efraimidis-Spirakis algorithm
4. Assigns seats in two waves (Wave 1: unique participants, Wave 2: fill remaining)
5. Exports results to formatted Excel with per-workshop sheets
