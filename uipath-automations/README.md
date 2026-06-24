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

## How to run on the work PC

> **API note (from the sanctioned Studio template):** this version targets elements via the
> **Object Repository** — `uiAutomation.Open(Descriptors.MyApp.FirstScreen)` →
> `screen.Click(Descriptors.MyApp.FirstScreen.SettingsButton)`. It does **not** use
> `Target.FromSelector("<uia .../>")` (that overload isn't exposed here — it's the likely cause
> of the earlier build errors). So you must **capture the elements once** before the code compiles.

### Step 1 — create the project + coded workflow
1. **UiPath Studio** (≥ 23.10) → **new Process** (generates a correct `project.json` for your version).
2. Confirm **`UiPath.UIAutomation.Activities` ≥ 23.10** is installed (Manage Packages).
3. Right-click project → **New → Coded Workflow** → name it `OpenCalcAndAdd`.
4. Copy **only the body of `Execute()`** from `OpenCalcAndAdd.cs` into the generated file, plus the
   extra `using` lines (`System`, `System.Globalization`, `System.Text.RegularExpressions`).
   (Keeping your generated namespace/class avoids any mismatch; `Descriptors` resolves in your project.)

### Step 2 — capture Calculator in the Object Repository (REQUIRED — this is what makes it compile)
Open the **Object Repository** panel, then:
1. **Create Application** → name it **`Calculator`** (point it at Calculator; set its launch path so `Open` can start it).
2. **Capture a Screen** under it → name it **`MainScreen`**.
3. **Indicate** these elements on the running Calculator and name them **exactly**:
   - the **9** button → `Nine`
   - the **+** button → `Plus`
   - the **=** button → `Equals`
   - the **results display** text → `Display`  *(AutomationId `CalculatorResults`)*

These generate `Descriptors.Calculator.MainScreen.Nine` etc., which the code references. (Name them
differently? Then just edit the descriptor paths in the `.cs` to match.)

### Step 3 — run
Run it. Watch Output for `Raw display:` / `Parsed result:`. No exception = pass; the assert throwing = fail.

The bundled `project.json` is **reference only** — versions won't match your sanctioned Studio. Prefer the generate-in-Studio route above.

---

## Likely tweaks when you run it (don't be surprised)

The UI Automation method surface shifts between versions — verify these lines against IntelliSense:
1. `uiAutomation.Open(...)` signature / return type.
2. ~~Whether `Click`/`GetText` take a selector string or a descriptor~~ → **resolved by the template: use Object Repository `Descriptors.*` (not `Target.FromSelector`).**
3. The log helper: `Log(...)` here vs `system.LogMessage(...)` in some versions.

**Calculator is UWP** (runs under `ApplicationFrameHost.exe`), so `Open("calc.exe")` may not attach cleanly. If it doesn't: capture the top-level window (title **"Calculator"**) in **UI Explorer** and open/attach by that descriptor.

---

## Selectors — the one thing that can't be written from memory

For native apps, every element needs a valid descriptor derived from the **running app's UIAutomation tree**. This sanctioned Studio uses the **Object Repository** path (per the template), so capture-once-then-reference (Step 2 above) is the way — and it's the same workflow you'll use for the real PUMA injection. (`Inspect.exe` / `FlaUInspect` are still handy to *peek* at AutomationIds while capturing, e.g. confirming the display is `CalculatorResults`.)

This is the **division of labor** that makes the code-first path work: the one-time element capture happens in the UI (clicking), everything else — sequence, the three-tier checks, loops, retries, Geneos output — is reviewable C# in this repo.

---

## 🛠 Troubleshooting — "Coded types may not be available in workflows due to errors"

That red line in the **Output** panel is only a **rollup**, not the real error. To see the actual cause: **View → Error List** (or double-click a red line). Then, in priority order:

| # | Symptom (in Error List) | Cause | Fix |
|---|---|---|---|
| **A** | `uiAutomation` does not exist / is not resolved; cascade of "type not found" | The **UI Automation package isn't installed** in this project (common when pasting into a blank Process) | **Manage Packages → install `UiPath.UIAutomation.Activities` (≥ 23.10) → rebuild.** The `uiAutomation` service accessor only exists once that package is referenced. (`system` works without it.) |
| **B** | `'NApplicationCard' does not contain a definition for 'Click'/'GetText'`, or `Target.FromSelector` overload not found | **Method surface differs by version** | Swap the flagged line to its **ALTERNATIVE (service form)** — each is commented inline in the `.cs`. Let IntelliSense confirm the overload. |
| **C** | (compiles, but) fails at runtime to find/attach the app | **Calculator is UWP** (`ApplicationFrameHost.exe`); `Open("calc.exe")` may not attach | Capture the top-level window (title **"Calculator"**) in **UI Explorer**, open/attach by that descriptor. |
| — | `No connection to Integration Service…` | benign | **Ignore** — not needed for local UI automation. |

**Fastest way to send me the real error:** double-click the first red entry in the Error List and paste its exact text (e.g. `CS0103: The name 'uiAutomation' does not exist...`). The `CSxxxx` code + message tells me precisely which line/overload to fix.

> Note: the API surface is version-specific, so the exact method overloads depend on your sanctioned Studio/package versions. The inline ALTERNATIVE forms cover the common variations; the Error List text lets me pin the exact one.

## Next step (proposed)

Extend this into the **three-tier scaffold** — *reachable → responding correctly → financially coherent* — with each layer stubbed and the **Geneos output hook** wired in, same Coded Workflow structure. Then port the pattern to the real PUMA injection (config-driven, 6 PDC instances).

*Aside: UiPath ships official "UiPath for Coding Agents" skills (Claude Code / Copilot / Cursor / Gemini), but the sanctioned in-environment agent tool here is **GitLab Duo** — so the agent integration may not be greenlit. Doesn't matter for these files: they're just C# in the repo regardless.*
