using System;
using System.Diagnostics.CodeAnalysis;

namespace Synthesis.Bethesda.Execution.Reporters
{
    [ExcludeFromCodeCoverage]
    public class ThrowReporter : IRunReporter
    {
        public static ThrowReporter Instance = new();

        private ThrowReporter()
        {
        }

        public void ReportOverallProblem(Exception ex)
        {
            throw ex;
        }

        public void ReportPrepProblem(object? key, string name, Exception ex)
        {
            throw ex;
        }

        public void ReportRunProblem(object? key, string name, Exception ex)
        {
            throw ex;
        }

        public void ReportStartingRun(object? key, string name)
        {
        }

        public void ReportRunSuccessful(object? key, string name, string outputPath)
        {
        }

        public void Write(object? key, string? name, string str)
        {
        }

        public void WriteError(object? key, string? name, string str)
        {
        }
    }
}
