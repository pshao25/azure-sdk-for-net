// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BenchmarkDotNet.Running;

namespace Compute.Init.Perf;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--validate"])
        {
            VirtualMachineInitTests.Validate();
            foreach (string format in new[] { "J", "W" })
            {
                new VirtualMachineSerializationBenchmarks { Format = format }.Setup();
            }
            Console.WriteLine("Compute VM init accessors, object initialization, J/W visibility, parity, and round trips passed.");
            return;
        }

        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        if (summaries.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.Success)))
        {
            Environment.ExitCode = 1;
        }
    }
}
