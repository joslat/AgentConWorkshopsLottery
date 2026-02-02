# 003 - Developer Agent Creation

**Date:** 2026-02-02  
**Type:** Agent Configuration Prompt  
**Status:** Completed

---

## Original Request

Create a developer agent to code this solution using the principles specified, review the architecture, ADRs and Implementation plans to create the most fitting agent for this. Create the agent in `/.github/agents` and if deemed necessary any custom instructions that can help it. The developer agent should use Claude Opus 4.5 always.

Add this prompt to the prompt section as a markdown document as usual. Engrave this into the developer agent as well - any request goes to prompts.

---

## Analysis Performed

Before creating the agent, the following documents were reviewed:

### Architecture Document
- `docs/ARCHITECTURE.md` - Full system specification including domain models, component architecture, algorithms, and data flow

### ADR Documents
- `ADR-001` - Single project structure decision
- `ADR-002` - ClosedXML for Excel operations
- `ADR-003` - Efraimidis-Spirakis algorithm for weighted lottery
- `ADR-004` - System.CommandLine for CLI
- `ADR-005` - Fuzzy column matching strategy
- `ADR-006` - Wave-based assignment strategy

### Implementation Plans
- `001` - Project setup & domain models
- `002` - Excel parser service
- `003` - Validation service & ranking parser
- `004` - Lottery engine
- `005` - Excel writer, CLI & orchestrator

---

## Deliverables Created

### 1. Main Agent Definition
**Location:** `.github/agents/workshop-lottery-developer.md`

Contains:
- Agent identity and purpose
- Core principles (PRAGMATIC, KISS, DRY, SOLID, CLEAN)
- Project context and structure
- Technology stack reference
- Mandatory behaviors including prompt documentation requirement
- Domain knowledge summary
- Quick reference commands

### 2. Global Copilot Instructions
**Location:** `.github/copilot-instructions.md`

Contains:
- Model preference (Claude Opus 4.5)
- Key documentation links
- Technology decisions summary table
- Code conventions
- Quick context overview

### 3. Supplementary Development Guide
**Location:** `.github/instructions/development-guide.md`

Contains:
- Phase-specific implementation guidance
- Common code patterns
- Testing patterns
- Error message conventions
- Console output format
- File naming conventions

---

## Key Agent Features

### 1. Claude Opus 4.5 Model Preference
The agent is configured to always use Claude Opus 4.5 for maximum capability.

### 2. Documentation-First Approach
The agent is instructed to:
- Read ARCHITECTURE.md before implementing features
- Review applicable ADRs for context
- Follow implementation plans phase by phase

### 3. Mandatory Prompt Documentation
**Critical behavior engrained:** Every user request must be documented in `docs/prompts/NNN-description.md`. This ensures:
- Complete audit trail
- Context preservation
- Decision transparency

### 4. Test-Driven Development
The agent is configured to write tests alongside or before implementation.

### 5. Alignment Verification
For each task, the agent must verify:
- Alignment with ARCHITECTURE.md
- Compliance with relevant ADRs
- Following the implementation plan

---

## File Structure Created

```
.github/
├── copilot-instructions.md          # Global Copilot instructions
├── agents/
│   └── workshop-lottery-developer.md # Main developer agent definition
└── instructions/
    └── development-guide.md          # Supplementary development guide
```

---

## Usage

The developer agent can be invoked in VS Code with GitHub Copilot Chat by referencing the workspace context. The global copilot-instructions.md will automatically apply to all Copilot interactions in this repository.

To use the agent effectively:
1. Start with a clear task description
2. Reference the relevant implementation phase
3. The agent will automatically create a prompt file for the request
4. Implementation follows the established architecture and ADRs

---

## Notes

- The `.github/agents/` folder structure follows the emerging pattern for GitHub Copilot custom agents
- The `copilot-instructions.md` at the `.github/` root provides workspace-wide context
- Additional instructions can be added to `.github/agents/instructions/` as needed
