// ─────────────────────────────────────────────────────────────────────────────
//  OpenCalcAndAdd.cs — UiPath Coded Workflow (proof-of-path)
//
//  Purpose: prove the CODE-FIRST UiPath path works before touching PUMA.
//  Flow: open Calculator -> click 999 + 999 = -> read display -> parse -> log -> assert.
//  The final assert is a baby version of the tier-3 "financially coherent" check
//  (we don't just confirm the app responded — we confirm the *answer* is correct).
//
//  PREREQUISITES (see README.md):
//   - UiPath Studio >= 23.10 (Coded Workflows need System.Activities + UIAutomation.Activities >= 23.10).
//     ❗ OPEN QUESTION: confirm the sanctioned Studio version with Pawan. If it's < 23.10,
//        this path is closed and we fall back to the drag-and-drop canvas.
//   - Add this file to a UiPath project as a Coded Workflow (right-click > New > Coded Workflow),
//     or paste its body into a generated coded-workflow file so the SDK references resolve.
//
//  VERIFY AGAINST INTELLISENSE (the UI Automation method surface shifts between versions —
//  these are the lines most likely to need a tweak on the work PC):
//   1. uiAutomation.Open(...) signature/return type.
//   2. Whether Click/GetText take Target.FromSelector("<uia .../>") or a captured descriptor.
//   3. The log helper: Log(...) here vs system.LogMessage(...) in some versions.
//
//  CALCULATOR NOTE: Windows Calculator is UWP (runs under ApplicationFrameHost.exe), so
//  Open("calc.exe") may not attach cleanly. If it doesn't, capture the top-level window
//  (window title "Calculator") in UI Explorer and open/attach by that descriptor instead.
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
            var calc = uiAutomation.Open("calc.exe");

            string[] sequence =
            {
                "num9Button", "num9Button", "num9Button",
                "plusButton",
                "num9Button", "num9Button", "num9Button",
                "equalButton"
            };

            foreach (var automationId in sequence)
                calc.Click(Target.FromSelector($"<uia automationid='{automationId}' />"));

            string displayText = calc.GetText(
                Target.FromSelector("<uia automationid='CalculatorResults' />"));

            string digitsOnly = Regex.Replace(displayText, "[^0-9]", "");
            int result = int.Parse(digitsOnly, CultureInfo.InvariantCulture);

            Log($"Raw display: {displayText}");
            Log($"Parsed result: {result}");

            if (result != 1998)
                throw new InvalidOperationException($"Expected 1998, got {result}.");
        }
    }
}
