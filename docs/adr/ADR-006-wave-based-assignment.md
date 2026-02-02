# ADR-006: Wave-Based Assignment Strategy

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Fair seat distribution algorithm design

---

## Context

The lottery must balance two competing goals:
1. **Maximize unique participants:** Give as many people as possible at least one workshop seat
2. **Fill all seats:** Ensure workshops aren't under-subscribed

A single-pass algorithm might fill one workshop with people who also have high priority for other workshops, reducing overall participant diversity.

## Decision Drivers

- Fairness (maximize unique attendees first)
- Full utilization (don't leave seats empty)
- Transparency (explainable to participants)
- Implementation simplicity

## Options Considered

### Option 1: Two-Wave Assignment (Selected)
- Wave 1: Assign seats only to people without any assignment yet
- Wave 2: Fill remaining seats with anyone eligible

### Option 2: Global Optimization
- Integer linear programming to maximize unique attendees
- Complex, harder to explain, overkill for scale

### Option 3: Single Pass per Workshop
- Simple but may give same people multiple seats while others get none

### Option 4: Round-Robin Across Workshops
- Alternate assigning one seat per workshop
- Fairer but more complex ordering logic

## Decision

**Option 1: Two-Wave Assignment**

## Rationale

1. **Clear Fairness Model:**
   - Wave 1 guarantees maximum unique participants
   - Wave 2 rewards people who ranked multiple workshops

2. **Easy to Explain:**
   - "First, everyone gets one seat if possible"
   - "Then, remaining seats go to next-best candidates"

3. **Auditable:**
   - Output shows Wave column
   - Participants can verify fairness

4. **Optimal for Unique Coverage:**
   - Mathematically maximizes distinct participants in Wave 1
   - Only after exhausting unique candidates do we allow doubles

## Algorithm Detail

```
WAVE 1: Unique Assignment
assignedPeople = {}

for workshop in [W1, W2, W3]:  // configurable order
    workshop.accepted = []
    
    for person in workshop.weightedOrder:
        if |workshop.accepted| >= capacity:
            break
        if person NOT in assignedPeople:
            workshop.accepted.add((person, wave=1))
            assignedPeople.add(person)

WAVE 2: Fill Remaining
for workshop in [W1, W2, W3]:
    openSeats = capacity - |workshop.accepted|
    
    for person in workshop.weightedOrder:
        if openSeats == 0:
            break
        if person NOT in workshop.accepted:  // might be assigned elsewhere
            workshop.accepted.add((person, wave=2))
            openSeats--

    workshop.waitlist = remaining candidates not in workshop.accepted
```

## Example Scenario

```
Capacity: 2 seats per workshop
3 people: Alice, Bob, Carol

Workshop 1 weighted order: [Alice, Bob, Carol]
Workshop 2 weighted order: [Alice, Carol, Bob]

WAVE 1:
- W1: Alice (new) ✓, Bob (new) ✓ → Full
- W2: Alice (already assigned) ✗, Carol (new) ✓ → 1 seat filled

After Wave 1: 3 unique people have seats (optimal)

WAVE 2:
- W2: Alice (not in W2 accepted) ✓ → Full

Final:
- W1: Alice(W1), Bob(W1)
- W2: Carol(W1), Alice(W2)

Alice gets 2 workshops, everyone gets at least 1.
```

## Consequences

### Positive
- Maximizes participant diversity
- Simple two-pass implementation
- Transparent output
- Configurable workshop order for priority control

### Negative
- Workshop order affects outcomes (noted in output)
- Wave 2 winners may seem "luckier" than Wave 1 non-winners

### Mitigations
- Document that workshop order is configurable
- Output shows wave for transparency
- Summary stats show wave distribution

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Algorithm in Section 5.2
