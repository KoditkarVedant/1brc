using System.IO.MemoryMappedFiles;
using System.Text;

namespace OneBRC.ParallelMemoryMappedFile;

public class ParallelMemoryMappedFile : ISolution
{
    private const int DictInitCapacity = 10_000;
    private const int MaxChunkSize = int.MaxValue - 100_000;

    public void Process(string filePath)
    {
        List<(long start, int length)> chunks;
        long length;

        using (var fileStream = File.OpenRead(filePath))
        {
            chunks = SplitIntoMemoryChunks(fileStream);
            fileStream.Position = 0;
            length = fileStream.Length;
        }

        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
        using var accessor = mmf.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read);
        var result = chunks
            .Where(x => x.length > 0)
            .AsParallel()
            .Select(t => ProcessChunk(accessor, t.start, t.length))
            .ToList()
            .Aggregate((result, chunk) =>
            {
                foreach (var pair in chunk)
                {
                    if (result.TryGetValue(pair.Key, out var summary))
                    {
                        summary.Merge(pair.Value);
                    }
                    else
                    {
                        result[pair.Key] = pair.Value;
                    }
                }

                return result;
            });

        // Print Result
        Console.Write("{");
        foreach (var pair in result)
        {
            Console.Write($"{pair.Key}={pair.Value}");
        }

        Console.Write("}");
    }

    private static Dictionary<string, Summary> ProcessChunk(MemoryMappedViewAccessor accessor, long start, int length)
    {
        var memo = new Dictionary<string, Summary>(DictInitCapacity);

        var buffer = new byte[length];

        accessor.ReadArray(start, buffer, 0, length);

        int pos = 0;
        int i = 0;
        int semiColonPos = 0;
        while (pos < buffer.Length)
        {
            i = pos;
            semiColonPos = 0;

            while (pos < buffer.Length)
            {
                if (buffer[pos] == (byte)'\n')
                {
                    pos++;
                    break;
                }

                if (buffer[pos] == (byte)';')
                {
                    semiColonPos = pos;
                }

                pos++;
            }

            var name = Encoding.UTF8.GetString(buffer, i, semiColonPos - i);
            var temp = double.Parse(Encoding.UTF8.GetString(buffer.AsSpan(semiColonPos + 1, pos - (semiColonPos + 2))));

            if (memo.ContainsKey(name) == false)
            {
                memo[name] = new Summary(temp);
            }
            else
            {
                memo[name].Add(temp);
            }
        }

        return memo;
    }

    private static List<(long start, int length)> SplitIntoMemoryChunks(FileStream fileStream)
    {
        var fileLength = fileStream.Length;

        var chunkCount = Math.Max(1, Environment.ProcessorCount);
        var chunkSize = fileLength / chunkCount;

        while (chunkSize > MaxChunkSize)
        {
            chunkCount *= 2;
            chunkSize = fileLength / chunkCount;
        }

        var chunks = new List<(long start, int length)>(chunkCount);

        long currPos = 0;
        for (var i = 0; i < chunkCount; i++)
        {
            if (currPos + chunkSize >= fileLength)
            {
                chunks.Add((currPos, (int)(fileLength - currPos)));
                break;
            }

            fileStream.Position = currPos + chunkSize;

            int ch;
            while ((ch = fileStream.ReadByte()) >= 0 && ch != '\n')
            {
            }

            var len = fileStream.Position - currPos;
            chunks.Add((currPos, (int)len));

            currPos = fileStream.Position;
        }

        var previous = chunks[0];
        for (int i = 1; i < chunks.Count; i++)
        {
            var current = chunks[i];

            if (previous.start + previous.length != current.start)
            {
                throw new Exception("Bad chunks");
            }

            if (i == chunks.Count - 1 && current.start + current.length != fileLength)
                throw new Exception("Bad last chunks");

            previous = current;
        }

        return chunks;
    }

    private class Summary
    {
        private double _min;
        private double _max;
        private double _sum;
        private int _count;

        public Summary(double value)
        {
            _min = value;
            _max = value;
            _sum = value;
            _count = 1;
        }

        public void Add(double value)
        {
            _min = Math.Min(_min, value);
            _max = Math.Min(_max, value);
            _sum += value;
            _count++;
        }

        public void Merge(Summary summary)
        {
            _min = Math.Min(_min, summary._min);
            _max = Math.Min(_max, summary._max);
            _sum += summary._sum;
            _count += summary._count;
        }

        private double Average => _sum / _count;

        public override string ToString()
        {
            return $"{_min / 10.0:N1}/{Average / 10.0:N1}/{_max / 10.0:N1}";
        }
    }

    public void Dispose()
    {
    }
}