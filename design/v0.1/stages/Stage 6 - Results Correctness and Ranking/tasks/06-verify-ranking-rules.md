# Task 06: Verify Ranking Rules

## Goal

Verify correctness and ranking behavior against Stage 6 rules.

## Work

- Test fully correct submission.
- Test partially correct submission.
- Test higher correctness ranking above lower correctness.
- Test equal correctness ranking by shorter spent time.
- Test deterministic final tie-breaker if correctness and spent time tie.
- Confirm unsubmitted players are not ranked.

## Done Criteria

- Ranking favors higher correctness.
- Ranking breaks ties by shorter spent time.
- Unsubmitted players do not appear in the ranked result list.
- Correctness values are accurate.

## Verification

- Run the web app and test with one master tab and at least two player tabs.
