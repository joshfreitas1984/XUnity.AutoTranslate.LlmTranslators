using Xunit;

// Configuration path regressions can affect the process-wide current directory.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
