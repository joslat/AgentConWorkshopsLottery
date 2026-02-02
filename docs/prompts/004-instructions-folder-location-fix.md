# 004 - Instructions Folder Location Fix

**Date:** 2026-02-02  
**Type:** Structure Correction  
**Status:** Completed

---

## Original Request

The user noted that the instructions folder should be under `.github/` directly, not under `.github/agents/`.

Corrected structure from:
```
.github/
└── agents/
    ├── workshop-lottery-developer.md
    └── instructions/
        └── development-guide.md
```

To:
```
.github/
├── copilot-instructions.md
├── agents/
│   └── workshop-lottery-developer.md
└── instructions/
    └── development-guide.md
```

---

## Actions Taken

1. Created `.github/instructions/development-guide.md` with the same content
2. Removed `.github/agents/instructions/` folder
3. Updated prompt 003 documentation to reflect correct paths

---

## Note

This prompt was missing from the documentation and has been added retroactively to maintain the complete audit trail.
