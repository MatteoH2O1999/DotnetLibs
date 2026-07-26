// Copyright (C) 2025-2026 Matteo Dell'Acqua
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using static Crayon.Output;

namespace TestLogger
{
    [FriendlyName("TestLogger")]
    [ExtensionUri("logger://TestLogger/v1")]
    public class TestLogger : ITestLoggerWithParameters
    {
        private const string TEST_RUN_DIRECTORY = "TestRunDirectory";

        public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters)
        {
            events.TestResult += (sender, results) => this.TestFinished(results);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            this.Initialize(events, new Dictionary<string, string?>() { { TEST_RUN_DIRECTORY, testRunDirectory } });
        }

        private void TestFinished(TestResultEventArgs resultEvent)
        {
            string testClassName = TestClassName(resultEvent.Result.TestCase.FullyQualifiedName);

            StringBuilder message = new("  ");

            switch (resultEvent.Result.Outcome)
            {
                case TestOutcome.Passed:
                    message.Append(Bright.Green("\u2713 Passed"));
                    break;
                case TestOutcome.Failed:
                    message.Append(Bright.Red("\u2718 Failed"));
                    break;
                case TestOutcome.Skipped:
                    message.Append(Rgb(223, 142, 20).Text("\u21A9 Skipped"));
                    break;
                case TestOutcome.NotFound:
                    message.Append(Background.Red().White("\u26A0 Not found"));
                    break;
                default:
                    throw new InvalidOutcomeException(resultEvent.Result.Outcome);
            }
            message.Append(' ');

            message.Append(testClassName.Substring(0, testClassName.Length - 4));
            message.Append(": ");
            message.Append(resultEvent.Result.DisplayName);

            message.Append(" [");
            message.Append(Duration(resultEvent.Result.Duration));
            message.Append(']');

            if (
                resultEvent.Result.Outcome == TestOutcome.Failed
                && resultEvent.Result.ErrorMessage is not null
                && resultEvent.Result.ErrorStackTrace is not null
            )
            {
                message.Append("\n    ");
                message.Append(Bright.Red(resultEvent.Result.ErrorMessage.Replace("\n", "\n      ")));
                message.Append("\n    ");
                message.Append(Bright.Red(resultEvent.Result.ErrorStackTrace.Replace("\n", "\n      ")));
            }

            Console.WriteLine(message);
        }

        private static string TestClassName(string fullyQualifiedName)
        {
            string[] split = fullyQualifiedName.Split('.');
            string candidate = split[split.Length - 2];
            return candidate.ToLower().EndsWith("test")
                ? candidate
                : throw new TestClassNotFoundException(fullyQualifiedName);
        }

        private static string Duration(TimeSpan duration)
        {
            if (duration.TotalSeconds > 1)
            {
                return $"{duration.Seconds} s";
            }
            else if (duration.TotalMilliseconds < 1)
            {
                return "< 1 ms";
            }
            return $"{duration.Milliseconds} ms";
        }

        private class TestClassNotFoundException(string fullyQualifiedTest)
            : Exception($"Could not find test class from fully qualified name '{fullyQualifiedTest}'") { }

        private class InvalidOutcomeException(TestOutcome outcome) : Exception($"Invalid test outcome: {outcome}") { }
    }
}
