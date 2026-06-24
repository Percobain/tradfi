// ─────────────────────────────────────────────────────────────────────────────
//  OpenCalcAndAdd.cs — UiPath Coded Workflow (proof-of-path)
//
//  Purpose: prove the CODE-FIRST UiPath path works before touching PUMA.
//  Flow: open Calculator -> click 999 + 999 = -> read display -> parse -> log -> assert.
//  The final assert is a baby version of the tier-3 "financially coherent" check.
//
//  ──────────────────────────────────────────────────────────────────────────
//  ❗ IF YOU GOT "Coded types may not be available in workflows due to errors":
//     That is the Output rollup, not the real error. Open  View > Error List
//     (or double-click a red line) to see the actual compiler message, then
//     check these IN ORDER — most likely first:
//
//   (A) `uiAutomation` doesn't resolve  ->  the UI Automation package is NOT
//       installed in THIS project. Fix: Manage Packages > install
//       `UiPath.UIAutomation.Activities` (>= 23.10) > rebuild. (Most common cause
//       when you paste this into a blank Process.) `system` works without it; the
//       UI Automation service accessor only exists once that package is referenced.
//
//   (B) A method signature differs by version. The lines flagged "VERIFY" below are
//       the usual suspects. Each has an ALTERNATIVE form commented next to it — if
//       the primary shows a red squiggle, swap to the alternative (IntelliSense will
//       confirm which overload your installed version exposes).
//
//   (C) Calculator is UWP (runs under ApplicationFrameHost.exe) so Open("calc.exe")
//       may compile but fail to ATTACH at runtime. If so, capture the top-level
//       window (title "Calculator") in UI Explorer and open/attach by that descriptor.
//  ──────────────────────────────────────────────────────────────────────────
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;

namespace TradFi
{
    public class OpenCalcAndAdd : CodedWorkflow
    {
        [Workflow]
        public void Execute()
        {
            // VERIFY (A): if `uiAutomation` is unresolved -> install UiPath.UIAutomation.Activities.
            var calc = uiAutomation.Open("calc.exe");

            string[] sequence =
            {
                "num9Button", "num9Button", "num9Button",
                "plusButton",
                "num9Button", "num9Button", "num9Button",
                "equalButton"
            };

            // VERIFY (B): primary is the fluent app form `calc.Click(Target...)`.
            // ALTERNATIVE (service form): uiAutomation.Click(Target.FromSelector($"<uia automationid='{automationId}' />"));
            foreach (var automationId in sequence)
                calc.Click(Target.FromSelector($"<uia automationid='{automationId}' />"));

            // VERIFY (B): primary `calc.GetText(Target...)`.
            // ALTERNATIVE (service form): uiAutomation.GetText(Target.FromSelector("<uia automationid='CalculatorResults' />"));
            string displayText = calc.GetText(
                Target.FromSelector("<uia automationid='CalculatorResults' />"));

            string digitsOnly = Regex.Replace(displayText, "[^0-9]", "");
            int result = int.Parse(digitsOnly, CultureInfo.InvariantCulture);

            // VERIFY (B): `Log(...)` is provided by the CodedWorkflow base class.
            // ALTERNATIVE: system.LogMessage(displayText);  (or Log(LogLevel.Info, "..."))
            Log($"Raw display: {displayText}");
            Log($"Parsed result: {result}");

            if (result != 1998)
                throw new InvalidOperationException($"Expected 1998, got {result}.");
        }
    }
}
