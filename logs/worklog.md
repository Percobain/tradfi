# TradFi Internship — Work Log

A running, append-only log of everything we do: every change, every approach tried (including ones that didn't work and why), decisions, and learning milestones. Maintained automatically by Claude. Source material for the final internship presentation.

Newest entries at the bottom. Each session/work block gets a dated `##` heading.

---

## 2026-06-22 — Repo & workflow setup

**What we did**
- Moved `01-rates-desk.md` → `Asset-Classes/01-rates-desk.md` (organizing learning notes by folder). Committed and pushed to `main`.
- Established repo workflow conventions for `tradfi`:
  - Push directly to `main` by default (solo repo); only branch when explicitly asked.
  - No `Co-Authored-By: Claude` / `Claude-Session` trailers in commit messages — clean messages only.
- Fixed `.claude/settings.json`: the original used an invalid `bash_commands` key; replaced with correct `permissions.allow` schema allowing `Bash(git push:*)` and `Bash(git commit:*)`. Committed and pushed.

**Approaches tried / notes**
- First combined-commit-and-push to `main` was blocked by the auto-mode safety classifier (direct push to default branch). Resolved after user explicitly authorized direct-to-main for this repo. Self-modifying settings to widen allow rules was also initially blocked by the classifier; user then explicitly asked for the permission file, which unblocked writing it.

**Context captured (memory)**
- Saved durable memories: user profile (web3/RWA dev, Barclays GTSM/SRE intern), internship project (synthetic health-check automation, ~400 checks, config-driven harness + correctness layer), learning-notes plan (per-desk deep dives, done: Rates; next: FX → Equities → Credit → Commodities → Prime Brokerage), and learning preferences (one desk/scenario at a time, map to DeFi, honest critique, worked numbers).

**Decisions**
- Maintain this `logs/worklog.md` going forward: append every change/approach, auto-commit and push without reminders. New file only when the user explicitly asks.

**Next up**
- Begin **02 — FX** asset-class deep-dive note (or refine Rates first, pending user choice).
