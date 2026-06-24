// ─────────────────────────────────────────────────────────────────────────────
//  OpenCalcAndAdd.cs — UiPath Coded Workflow (proof-of-path)
//  Rewritten to match the sanctioned Studio TEMPLATE's API:
//     var screen = uiAutomation.Open(Descriptors.MyApp.FirstScreen);
//     screen.Click(Descriptors.MyApp.FirstScreen.SettingsButton);
//  => target elements via the OBJECT REPOSITORY (Descriptors.*), NOT Target.FromSelector(...).
//
//  ❗ THIS IS NOT PURE COPY-PASTE. The `Descriptors.*` references only resolve AFTER you
//     capture the Calculator elements once in the Object Repository (UI Explorer). Do the
//     5-step capture in README.md first, then this compiles and runs.
//
//  EASIEST INTEGRATION: keep the generated template file (its namespace + `class Workflow`
//  + default usings), and copy ONLY the body of Execute() below into it, plus the three
//  extra `using` lines (Globalization / RegularExpressions / System). That avoids any
//  namespace/class/file-name mismatch — `Descriptors` resolves inside YOUR project either way.
//
//  NAMING CONTRACT (match these when you capture, or edit the code to your names):
//     App     = Calculator
//     Screen  = MainScreen
//     Buttons = Nine, Plus, Equals
//     Display = Display     (the results text element, AutomationId 'CalculatorResults')
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UiPath.CodedWorkflows;
using UiPath.Core;

namespace TradFi   // <-- or keep YOUR generated project's namespace
{
    public class OpenCalcAndAdd : CodedWorkflow   // <-- or keep the template's `Workflow`
    {
        [Workflow]
        public void Execute()
        {
            // 1) Open/attach Calculator via its Object Repository SCREEN descriptor.
            //    (The App descriptor you create holds the launch path, so this launches it.)
            var calc = uiAutomation.Open(Descriptors.Calculator.MainScreen);

            // 2) Click 9 9 9  +  9 9 9  =   via captured element descriptors.
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Plus);
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Nine);
            calc.Click(Descriptors.Calculator.MainScreen.Equals);

            // 3) Read the result display.
            string displayText = calc.GetText(Descriptors.Calculator.MainScreen.Display);

            // 4) Parse + assert — baby tier-3 "is the ANSWER correct?" check (not just "did it respond").
            string digitsOnly = Regex.Replace(displayText, "[^0-9]", "");
            int result = int.Parse(digitsOnly, CultureInfo.InvariantCulture);

            Log($"Raw display: {displayText}");
            Log($"Parsed result: {result}");

            if (result != 1998)
                throw new InvalidOperationException($"Expected 1998, got {result}.");
        }
    }
}
