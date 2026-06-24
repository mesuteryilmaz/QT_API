using System;
using System.Collections.Generic;

namespace MBO_Market_Data_Analytics.Tests
{
    /// <summary>
    /// Minimal dependency-free assertion + runner. Each test calls the static asserts; the process
    /// exit code is 0 when every assertion passed and 1 otherwise.
    /// </summary>
    public static class TestHarness
    {
        private static int passed;
        private static int failed;
        private static readonly List<string> failures = new();
        private static string current = "";

        public static void Begin(string testName)
        {
            current = testName;
            Console.WriteLine($"- {testName}");
        }

        public static void IsTrue(bool cond, string what)
        {
            if (cond) { passed++; }
            else
            {
                failed++;
                string msg = $"{current}: {what}";
                failures.Add(msg);
                Console.WriteLine($"    FAIL: {what}");
            }
        }

        public static void AreEqual(int expected, int actual, string what)
            => IsTrue(expected == actual, $"{what} (expected {expected}, got {actual})");

        public static void AreEqual(double expected, double actual, double tol, string what)
            => IsTrue(Math.Abs(expected - actual) <= tol, $"{what} (expected {expected}, got {actual})");

        public static int Main()
        {
            Console.WriteLine("DataAnalytics deterministic test suite\n");

            try
            {
                AdaptiveParameterControllerTests.RunAll();
                MboOrderBookTests.RunAll();
                FakesSmokeTests.RunAll();
            }
            catch (Exception ex)
            {
                failed++;
                failures.Add($"UNHANDLED: {ex}");
                Console.WriteLine($"\nUNHANDLED EXCEPTION: {ex}");
            }

            Console.WriteLine($"\n==== {passed} passed, {failed} failed ====");
            if (failed > 0)
            {
                Console.WriteLine("Failures:");
                foreach (var f in failures) Console.WriteLine("  - " + f);
            }
            return failed == 0 ? 0 : 1;
        }
    }
}
