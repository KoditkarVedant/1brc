namespace OneBRC.Naive;

public class Naive : ISolution
{
    private const int DictInitCapacity = 10_000;

    public void Process(string filePath)
    {
        var memo = new Dictionary<string, Summary>(DictInitCapacity);
        using (var reader = File.OpenText(filePath))
        {
            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                {
                    break;
                }

                var semiColon = row.IndexOf(";", StringComparison.Ordinal);
                var name = row[..semiColon];
                var temp = double.Parse(row[(semiColon + 1)..]);

                if (memo.ContainsKey(name) == false)
                {
                    memo[name] = new Summary(temp);
                }
                else
                {
                    memo[name].Add(temp);
                }
            }
        }

        // Print Result
        Console.Write("{");
        foreach (var pair in memo)
        {
            Console.Write($"{pair.Key}={pair.Value}");
        }

        Console.Write("}");
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