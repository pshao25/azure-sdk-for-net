// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BenchmarkDotNet.Running;

namespace Compute.Virtual.Perf;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--validate"])
        {
            foreach (string format in new[] { "J", "W" })
            {
                new VirtualMachineSerializationBenchmarks { Format = format }.Setup();
            }
            new VirtualMachinePropertyReadBenchmarks().Setup();
            Console.WriteLine("Compute VM inheritance, virtual flags, nested values, J/W visibility and round-trip checks passed.");
            return;
        }
        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        if (summaries.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.Success)))
        {
            Environment.ExitCode = 1;
        }
    }
}
