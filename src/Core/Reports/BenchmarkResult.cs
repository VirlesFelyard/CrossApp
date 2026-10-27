namespace Core.Reports;

public sealed record BenchmarkResult(int Records, double WithoutToListMs, double WithToListMs);
