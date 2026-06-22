# TradFi Internship — Work Log

A running, append-only log of everything we do: every change, every approach tried (including ones that didn't work and why), decisions, and learning milestones. Maintained automatically by Claude. Source material for the final internship presentation and weekly progress reports.

**Format:** each entry is timestamped `[YYYY-MM-DD] [HH:MM TZ]: what happened`. Newest at the bottom. Grouped under a dated `##` day heading for easy weekly slicing.

---

## 2026-06-22

[2026-06-22] [12:38 IST]: Moved `01-rates-desk.md` into a new `Asset-Classes/` folder to organize learning notes by asset class. Committed and pushed to `main`.

[2026-06-22] [12:45 IST]: Established repo workflow conventions for `tradfi` — push directly to `main` by default (solo repo; branch only when explicitly asked); no `Co-Authored-By: Claude` / `Claude-Session` trailers in commit messages.

[2026-06-22] [12:50 IST]: Fixed `.claude/settings.json` — original had an invalid `bash_commands` key; replaced with correct `permissions.allow` schema allowing `Bash(git push:*)` and `Bash(git commit:*)`. Committed and pushed. (Note: an earlier combined commit-and-push to `main` and a settings-widening attempt were both blocked by the auto-mode safety classifier until the user explicitly authorized direct-to-main and the permission file.)

[2026-06-22] [12:55 IST]: Captured full internship context into durable memory — user profile (web3/RWA dev, Barclays GTSM/SRE intern), internship project (synthetic health-check automation, ~400 checks, config-driven harness + correctness layer), learning-notes plan (done: Rates; next: FX → Equities → Credit → Commodities → Prime Brokerage), and learning preferences (one desk/scenario at a time, map to DeFi, honest critique, worked numbers).

[2026-06-22] [12:58 IST]: Created this `logs/worklog.md` and made it a standing rule — append every change/approach automatically, auto-commit and push without reminders; same file unless told to start a new one. Committed and pushed.

[2026-06-22] [12:59 IST]: Switched log format to timestamped `[date] [time]:` entries so weekly progress reports can be sliced directly from the log.
