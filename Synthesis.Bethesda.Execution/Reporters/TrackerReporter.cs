using Noggog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Synthesis.Bethesda.Execution.Reporters
{
    [ExcludeFromCodeCoverage]
    public class TrackerReporter : IRunReporter
    {
        public bool Success => Overall == null
            && _prepProblems.Count == 0
            && StartingRun != null
            && RunProblem == null;

        public Exception? Overall { get; private set; }

        private readonly List<(string Patcher, Exception Exception)> _prepProblems = new();
        public IReadOnlyList<(string Patcher, Exception Exception)> PrepProblems => _prepProblems;

        public string? StartingRun { get; private set; }

        public (string Patcher, Exception Exception)? RunProblem { get; private set; }

        private readonly List<(string Patcher, FilePath OutputPath)> _patcherComplete = new();
        public IReadOnlyList<(string Patcher, FilePath OutputPath)> PatcherComplete => _patcherComplete;

        public void ReportOverallProblem(Exception ex)
        {
            if (Overall != null)
            {
                throw new ArgumentException("Reported two overall exceptions.");
            }
            Overall = ex;
        }

        public void ReportPrepProblem(object? key, string name, Exception ex)
        {
            _prepProblems.Add((name, ex));
        }

        public void ReportRunProblem(object? key, string name, Exception ex)
        {
            if (RunProblem != null)
            {
                throw new ArgumentException("Reported two name run exceptions.");
            }
            RunProblem = (name, ex);
        }

        public void ReportStartingRun(object? key, string name)
        {
            StartingRun = name;
        }

        public void ReportRunSuccessful(object? key, string name, string outputPath)
        {
            _patcherComplete.Add((name, outputPath));
        }

        public void Write(object? key, string? name, string str)
        {
        }

        public void WriteError(object? key, string? name, string str)
        {
        }
    }
}
