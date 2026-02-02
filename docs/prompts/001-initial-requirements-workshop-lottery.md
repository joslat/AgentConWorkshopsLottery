# 001 - Initial Requirements: Workshop Lottery System

**Date:** 2026-02-02  
**Type:** Requirements Prompt

---

## Original Request

Design a .NET 10 (C#) console based tool that imports a Microsoft Forms Excel and assigns workshop seats fairly using a weighted lottery with waves. Use ClosedXML for Excel I/O. Finally exports the results to excel.


Please read carefullz the requirements below and create:
1. an architecture document as specification of what is to be built, keepin in mind the concepts of DRY, KISS, SOLID, CLEAN and PRAGMATIC ARCHITECTURE. put it in /docs
2. Take the architectural decisions needed like what projects to create and why, what components to use and why, and store them as ADR documents under /docs/adr - after each update the architecture document if needed.
3. Divide what is to be done in progressive phases that can be achieved in a sesssion by you, my friendly coding agent using Claude Opus 4.5. write each as a fully contained implementation plan in /docs/plans and number them like 001-Implementation-plan-(add a suitable name).md - after each do a review to ensure this implementation plan is aligned with the architecture document, and if needed modify what makes most sense (if the approach from the implementation plan makes more sense that what is proposed in the architecture document and it is not a serious constraint, go ahead. otherwise realign the implementation plan to what is specified in the architecture document)
4. Add this prompt to /docs/prompts as 001-(description).md

Go you can do it, think carefully before doing and always review what you have done!

---

## INPUT
- Excel file (.xlsx) exported from MS Forms.
- Example columns (headers may vary slightly; match by prefix/contains, not exact string):
  - Full name
  - Email address
  - Will you bring a laptop (…)
  - Do you commit to be there 10 min before (…)
  - Do you want to attend Workshop 1 – "It's Giving Insecure Vibes…"
  - Do you want to attend Workshop 2 – "AI Architecture Critic…"
  - Do you want to attend Workshop 3 – "Build a Pizza Ordering Agent…"
  - Please rank the workshops you selected to attend.  (a single string with semicolon-separated items)

## WORKSHOP IDS
- W1 = Workshop 1 – Secure Coding Literacy for Vibe Coders
- W2 = Workshop 2 – AI Architecture Critic
- W3 = Workshop 3 – Build a Pizza Ordering Agent with Microsoft Foundry and MCP

## PARSE RULES
- Yes/No fields: case-insensitive; accept "Yes", "No" (optionally tolerate "Ja/Oui" but not required).
- Ranking field example:
  "Workshop 2 – AI Architecture Critic;Workshop 1 – Secure Coding…;Workshop 3 – Build a Pizza…"
  Parse by splitting on ';' and mapping each token to a workshop by detecting "Workshop 1", "Workshop 2", "Workshop 3".
  Compute rank position per workshop: 1/2/3.
  If a person requested a workshop = Yes but it doesn't appear in ranking, treat it as rank 3.

## ELIGIBILITY (hard filters)
- Globally eligible only if:
  - Laptop == Yes
  - Commit10Min == Yes
  - Name and Email are non-empty
- Duplicates: normalize email (trim + lowercase). If the same email appears more than once, DISQUALIFY ALL of them (per policy). Exclude them from assignment.

## CAPACITY
- 34 seats per workshop (configurable via CLI)

## FAIR SELECTION GOAL
- Weighted lottery based on ranking:
  - Rank1 weight=5
  - Rank2 weight=2
  - Rank3 weight=1
- Assignment in waves:
  Wave 1: prioritize giving as many unique people as possible at least one workshop seat.
  Wave 2: only if a workshop still has free seats, allow people already accepted elsewhere to fill remaining seats.

## WEIGHTED RANDOM PERMUTATION (reproducible)
- For each workshop, produce a deterministic weighted random order (permutation) of all eligible candidates who requested that workshop.
- Use a seed (CLI parameter --seed). Default seed: current date (YYYYMMDD) unless provided.
- Use Efraimidis–Spirakis method:
  For each candidate with weight w:
    u = random double in (0,1)
    score = log(u)/w
  Sort descending by score (higher = closer to 0 = better).
  This yields a weighted random permutation without replacement.

## ASSIGNMENT ALGORITHM
1) Load entries into an internal list:
   - Name, Email
   - Laptop (bool), Commit10Min (bool)
   - RequestedW1/W2/W3 (bool)
   - RankW1/RankW2/RankW3 (nullable int)
   - WeightW1/W2/W3 (int, based on rank)
2) Build per-workshop candidate lists: eligible AND requested == true.
3) For each workshop, generate WeightedOrder list using the method above.
4) Wave 1:
   - assignedPeople = empty set
   - for each workshop in configured order (default W1->W2->W3):
       accepted = []
       for person in WeightedOrder:
           if accepted.Count == capacity: break
           if person not in assignedPeople:
               accepted.Add(person); assignedPeople.Add(person); mark assignment (Wave=1)
       waitlist = remaining people in WeightedOrder excluding accepted, preserving order
5) Wave 2 (only if accepted.Count < capacity):
   - continue walking WeightedOrder and accept people not already accepted for that workshop (even if assigned elsewhere), mark Wave=2
6) Produce output workbook with exactly 3 sheets:
   - "Workshop 1", "Workshop 2", "Workshop 3"
   Each sheet rows in order:
     - Accepted first (in selection order), then Waitlist (in order)
   Columns:
     Order, Status (Accepted/Waitlist), Wave, Name, Email,
     Laptop, Commit10Min, RequestedThisWorkshop,
     RankForThisWorkshop, WeightUsed, RandomSeed
   Ensure stable formatting (header row bold, freeze top row).

## CLI
- Command: workshop-lottery
- Args:
  --input <path>
  --output <path> (default: WorkshopAssignments.xlsx)
  --capacity <int> (default 34)
  --seed <int> (optional)
  --order <W1,W2,W3> (optional, default W1,W2,W3)

## QUALITY / TESTS
- Include unit tests for:
  - ranking parsing
  - duplicate disqualification
  - weighted ordering determinism with a seed
  - wave 1 uniqueness constraint
- Log summary to console:
  total rows, eligible, disqualified, accepted per workshop, waitlist sizes.

## DELIVERABLES
- A ready-to-run .NET 10 solution:
  - /src console app
  - /tests unit tests
  - README with usage examples
