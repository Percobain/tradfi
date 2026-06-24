# uipath-automations

UiPath **Coded Workflows** (C#, `.cs`) for the PUMA DropCopy (PDC) synthetic-monitoring project. The code-first path: write `.cs` files with an `Execute()` entry point and call UI Automation APIs directly — no drag-and-drop canvas. They package/run like normal workflows and can be invoked from CI/CD via the UiPath CLI.

> **Why code-first:** the injection sequence, the three-tier health-check logic, loops, retries, and result formatting for the Geneos (ITRS) dataview all live as reviewable C# in the repo. Only the one-time **element capture** (selectors) happens in the UI.

---

## ❗ Confirm before investing time

**What UiPath Studio version is sanctioned in the Barclays GTSM environment?** Coded Workflows require **Studio ≥ 23.10** with `UiPath.System.Activities` + `UiPath.UIAutomation.Activities ≥ 23.10`. **If the locked build is < 23.10, this path is closed** and we fall back to the canvas. → Confirm with **Pawan**.

---

## Files

| File | What it is |
|---|---|
| `OpenCalcAndAdd.cs` | Proof-of-path demo: open Calculator, compute 999+999, read display, parse, log, **assert == 1998** (a baby tier-3 "is the answer correct?" check). |
| `project.json` | **Template** UiPath project manifest. Versions are placeholders — see note below. |

---

## How to run on the work PC (reliable path)

The **most reliable** way (avoids project.json version mismatches):

1. In **UiPath Studio** (≥ 23.10), create a **new Process** (this generates a correct `project.json` for your sanctioned version).
2. Ensure the **UI Automation** package is installed (Manage Packages → `UiPath.UIAutomation.Activities` ≥ 23.10).
3. Right-click the project → **New → Coded Workflow**. Name it `OpenCalcAndAdd`.
4. **Paste the body** of `OpenCalcAndAdd.cs` into the generated file (so the SDK references resolve against your installed packages).
5. **Run** it. Watch the Output panel for `Raw display:` / `Parsed result:` and a pass (no exception) / fail (assert throws).

The bundled `project.json` is a **convenience/reference template only** — its package versions almost certainly won't match your sanctioned Studio. Prefer the generate-in-Studio route above; use the template only if you want to open the folder directly as a project and then fix the versions.

---

## Likely tweaks when you run it (don't be surprised)

The UI Automation method surface shifts between versions — verify these lines against IntelliSense:
1. `uiAutomation.Open(...)` signature / return type.
2. Whether `Click` / `GetText` take `Target.FromSelector("<uia .../>")` or a **captured descriptor** (Object Repository).
3. The log helper: `Log(...)` here vs `system.LogMessage(...)` in some versions.

**Calculator is UWP** (runs under `ApplicationFrameHost.exe`), so `Open("calc.exe")` may not attach cleanly. If it doesn't: capture the top-level window (title **"Calculator"**) in **UI Explorer** and open/attach by that descriptor.

---

## Selectors — the one thing that can't be written from memory

For native apps, every element needs a valid selector derived from the **running app's UIAutomation tree** (AutomationIds / control IDs / class names). Two ways to get them:
1. **Object Repository** — capture each element once via **UI Explorer**, reference by name in code (recommended; survives churn better).
2. Hand-inspect with **Inspect.exe** / **FlaUInspect** and write selectors manually.

The Calculator AutomationIds used here (`num9Button`, `plusButton`, `equalButton`, `CalculatorResults`) are well-known and stable, which is why it's the proof-of-path target.

---

## Next step (proposed)

Extend this into the **three-tier scaffold** — *reachable → responding correctly → financially coherent* — with each layer stubbed and the **Geneos output hook** wired in, same Coded Workflow structure. Then port the pattern to the real PUMA injection (config-driven, 6 PDC instances).

*Aside: UiPath ships official "UiPath for Coding Agents" skills (Claude Code / Copilot / Cursor / Gemini), but the sanctioned in-environment agent tool here is **GitLab Duo** — so the agent integration may not be greenlit. Doesn't matter for these files: they're just C# in the repo regardless.*
