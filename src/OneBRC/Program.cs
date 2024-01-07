using System.Diagnostics;
using OneBRC;
using OneBRC.Dumb;
using OneBRC.Naive;
using OneBRC.ParallelMemoryMappedFile;

var solutions = new Dictionary<string, Type>()
{
    { "dumb", typeof(Dumb) },
    { "naive", typeof(Naive) },
    { "pmmf", typeof(ParallelMemoryMappedFile) }
};

if (args.Length != 2)
{
    Console.WriteLine("Usage: OneBRC <type> <measurements.txt>");
    return;
}

var type = args[0];
var filePath = args[1];

if (!solutions.TryGetValue(type, out var solutionType))
{
    Console.WriteLine($"Solution not found for type: {type}");
    return;
}

using var solution = (ISolution)Activator.CreateInstance(solutionType)!;

Console.WriteLine();
Console.WriteLine($"STARTING: {type} solution");
Console.WriteLine();

var sw = Stopwatch.StartNew();
solution.Process(filePath);
sw.Stop();

Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"DONE: Processed in {sw.Elapsed}");
Console.WriteLine();