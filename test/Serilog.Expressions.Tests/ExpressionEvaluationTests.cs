using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Runtime;
using Serilog.Expressions.Tests.Support;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class ExpressionEvaluationTests
    {
        static readonly string CasesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Cases");

        static IEnumerable<object[]> ReadCases(string filename)
        {
            foreach (var line in File.ReadLines(Path.Combine(CasesPath, filename)))
            {
                var cols = line.Split("⇶", StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length == 2)
                    yield return cols.Select(c => c.Trim()).ToArray<object>();
            }
        }

        public static IEnumerable<object[]> ExpressionEvaluationCases =>
            ReadCases("expression-evaluation-cases.asv");

        [Theory]
        [MemberData(nameof(ExpressionEvaluationCases))]
        public void ExpressionsAreCorrectlyEvaluated(string expr, string result)
        {
            var evt = Some.InformationEvent();
            
            evt.AddPropertyIfAbsent(
                new LogEventProperty("User", new StructureValue(new[]
                {
                    new LogEventProperty("Id", new ScalarValue(42)),
                    new LogEventProperty("Name", new ScalarValue("nblumhardt")), 
                })));
            
            var actual = SerilogExpression.Compile(expr)(evt);
            var expected = SerilogExpression.Compile(result)(evt);

            if (expected is null)
            {
                Assert.True(actual is null, $"Expected value: undefined{Environment.NewLine}Actual value: {Display(actual)}");
            }
            else
            {
                Assert.True(Coerce.IsTrue(RuntimeOperators._Internal_Equal(StringComparison.OrdinalIgnoreCase, actual, expected)), $"Expected value: {Display(expected)}{Environment.NewLine}Actual value: {Display(actual)}");
            }
        }

        static string Display(LogEventPropertyValue? value)
        {
            if (value == null)
                return "undefined";

            return value.ToString();
        }
    }
}
