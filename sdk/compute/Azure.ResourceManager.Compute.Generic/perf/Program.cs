// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BenchmarkDotNet.Running;

namespace Compute.Generic.Perf;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--validate"])
        {
            GenericModelFactoryTests.Validate();
            Console.WriteLine("Generic model creation, nested objects, collections, current templates, errors, and serialization parity passed.");
            return;
        }

        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        if (summaries.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.Success)))
        {
            Environment.ExitCode = 1;
        }
    }
}
