using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Transformations;

namespace Smoke.Readme;

// Runs the Quick Start examples from the root README against the packed Transformations meta package, the way a user
// would, and asserts the value each example's comment states. It also checks that no public type is defined in two
// Transformations assemblies, which is what made ConvertTo ambiguous (CS0121) for consumers.
internal static class Program
{
    private enum Status
    {
        Active,
        Inactive,
    }

    private enum MyEnum
    {
        [System.ComponentModel.Description("Described value")]
        Value,
    }

    private static (string Name, Func<bool> Run, string Expected) C(string name, Func<bool> run, string expected) => (name, run, expected);

    private static int Main()
    {
        var checks = new List<(string Name, Func<bool> Run, string Expected)>
        {
            C("\"42\".ConvertTo<int>()  // 42", () => "42".ConvertTo<int>() == 42, "42"),
            C("\"nope\".ConvertTo<int>(-1)  // -1", () => "nope".ConvertTo<int>(-1) == -1, "-1"),
            C("\"Hello World\".GetInitials()  // \"HW\"", () => "Hello World".GetInitials() == "HW", "HW"),
            C("\"test\".Repeat(3)  // \"testtesttest\"", () => "test".Repeat(3) == "testtesttest", "testtesttest"),
            C("\"<b>Hello</b>\".StripHtml()  // \"Hello\"", () => "<b>Hello</b>".StripHtml() == "Hello", "Hello"),
            C("\"child\".Pluralize(3)  // \"children\"", () => "child".Pluralize(3) == "children", "children"),
            C("\"person\".Pluralize(5)  // \"people\"", () => "person".Pluralize(5) == "people", "people"),
            C("\"Box\".Pluralize(2)  // \"Boxes\"", () => "Box".Pluralize(2) == "Boxes", "Boxes"),
            C("\"item\".Pluralize(1)  // \"item\"", () => "item".Pluralize(1) == "item", "item"),
            C("5.IsBetween(1, 10)  // true", () => 5.IsBetween(1, 10), "true"),
            C("MyEnum.Value.GetEnumDescription()  // reads [Description]", () => MyEnum.Value.GetEnumDescription() == "Described value", "Described value"),
            C("\"Active\".ToEnum<Status>()  // parses string to enum", () => "Active".ToEnum<Status>() == Status.Active, "Active"),
            C("HolidayHelper.GetEasterSunday(2024)  // 2024-03-31, Easter date calculation", () => HolidayHelper.GetEasterSunday(2024) == new DateTime(2024, 3, 31), "2024-03-31"),
            C("\"+1 (555) 111-2222\".IsSemanticMatch(\"15551112222\", PhoneNumber)  // true", () => "+1 (555) 111-2222".IsSemanticMatch("15551112222", SemanticType.PhoneNumber), "true"),
        };

        int failed = 0;
        foreach (var (name, run, expected) in checks)
        {
            string outcome;
            try
            {
                outcome = run() ? "PASS" : "FAIL";
            }
            catch (Exception ex)
            {
                outcome = $"FAIL ({ex.GetType().Name}: {ex.Message})";
            }

            if (outcome != "PASS")
            {
                failed++;
            }

            Console.WriteLine($"{outcome} {name}");
        }

        // Duplicate-type check: a public type name defined in more than one Transformations assembly.
        var definitions = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        // Every shipped Transformations assembly in the output folder, including the ones only referenced transitively.
        foreach (string path in Directory.GetFiles(AppContext.BaseDirectory, "Transformations*.dll"))
        {
            Assembly assembly = Assembly.LoadFrom(path);
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (!definitions.TryGetValue(type.FullName ?? type.Name, out List<string>? owners))
                {
                    definitions[type.FullName ?? type.Name] = owners = new List<string>();
                }

                owners.Add(assembly.GetName().Name ?? "?");
            }
        }

        string[] duplicates = definitions.Where(kv => kv.Value.Distinct().Count() > 1).Select(kv => $"{kv.Key} in {string.Join(", ", kv.Value.Distinct())}").ToArray();
        if (duplicates.Length == 0)
        {
            Console.WriteLine($"PASS no public type is defined in two Transformations assemblies ({definitions.Count} types checked)");
        }
        else
        {
            failed++;
            Console.WriteLine($"FAIL duplicate public types: {string.Join("; ", duplicates.Take(5))}");
        }

        Console.WriteLine($"{checks.Count + 1 - failed}/{checks.Count + 1} checks passed on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        return failed == 0 ? 0 : 1;
    }
}
