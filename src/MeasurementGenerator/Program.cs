using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public record WeatherStation(string Name, float AvgTemp);

class CreateMeasurements3
{
    public const int MaxNameLen = 100;
    public const int KeySetSize = 10_000;

    static void Main(string[] args)
    {
        int size = 0;
        if (args.Length != 1 || !int.TryParse(args[0], out size))
        {
            Console.WriteLine("Usage: CreateMeasurements.ps1 < number of records to create>");
            Environment.Exit(1);
        }

        var weatherStations = GenerateWeatherStations();
        var start = DateTime.Now;
        var rnd = new Random();

        using (var writer = new StreamWriter("measurements.txt"))
        {
            for (int i = 1; i <= size; i++)
            {
                var station = weatherStations[rnd.Next(KeySetSize)];
                double temp = rnd.NextGaussian(station.AvgTemp, 7.0);
                writer.Write(station.Name);
                writer.Write(';');
                writer.Write(Math.Round(temp * 10.0) / 10.0);
                writer.WriteLine();

                if (i % 50_000_000 == 0)
                {
                    Console.WriteLine($"Wrote {i:N0} measurements in {(DateTime.Now - start).TotalMilliseconds} ms");
                }
            }
        }
    }

    private static List<WeatherStation> GenerateWeatherStations()
    {
        var bigName = new StringBuilder(1 << 20);
        using (var rows = new StreamReader(File.OpenRead("data/weather_stations.csv")))
        {
            SkipComments(rows);
            while (true)
            {
                var row = rows.ReadLine();
                if (row == null)
                {
                    break;
                }
                bigName.Append(row, 0, row.IndexOf(';'));
            }
        }

        var weatherStations = new List<WeatherStation>();
        var names = new HashSet<string>();
        var minLen = int.MaxValue;
        var maxLen = int.MinValue;

        using (var rows = new StreamReader(File.OpenRead("data/weather_stations.csv")))
        {
            SkipComments(rows);
            var row = rows.ReadLine();
            using (var reader = new StringReader(bigName.ToString()))
            {
                var buf = new char[MaxNameLen];
                var rnd = new Random();
                const double yOffset = 4;
                const double factor = 2500;
                const double xOffset = 0.372;
                const double power = 7;

                for (int i = 0; i < KeySetSize; i++)
                {
                    var nameLen = (int)(yOffset + factor * Math.Pow(rnd.NextDouble() - xOffset, power));
                    var count = reader.Read(buf, 0, nameLen);
                    if (count == -1)
                    {
                        throw new Exception("Name source exhausted");
                    }

                    var nameBuf = new StringBuilder(nameLen);
                    nameBuf.Append(buf, 0, nameLen);

                    if (char.IsWhiteSpace(nameBuf[0]))
                    {
                        nameBuf[0] = ReadNonSpace(reader);
                    }

                    if (char.IsWhiteSpace(nameBuf[nameBuf.Length - 1]))
                    {
                        nameBuf[nameBuf.Length - 1] = ReadNonSpace(reader);
                    }

                    var name = nameBuf.ToString();

                    while (names.Contains(name))
                    {
                        nameBuf[rnd.Next(nameBuf.Length)] = ReadNonSpace(reader);
                        name = nameBuf.ToString();
                    }

                    int actualLen;
                    while (true)
                    {
                        actualLen = Encoding.UTF8.GetByteCount(name);
                        if (actualLen <= 100)
                        {
                            break;
                        }

                        nameBuf.Length--;
                        if (char.IsWhiteSpace(nameBuf[nameBuf.Length - 1]))
                        {
                            nameBuf[nameBuf.Length - 1] = ReadNonSpace(reader);
                        }

                        name = nameBuf.ToString();
                    }

                    if (name.IndexOf(';') != -1)
                    {
                        throw new Exception("Station name contains a semicolon!");
                    }

                    names.Add(name);
                    minLen = Math.Min(minLen, actualLen);
                    maxLen = Math.Max(maxLen, actualLen);
                    var lat = float.Parse(row.Substring(row.IndexOf(';') + 1));

                    var avgTemp = (float)(30 * Math.Cos(MathHelper.ToRadians(lat))) - 10;
                    weatherStations.Add(new WeatherStation(name, avgTemp));
                }
            }
        }

        Console.WriteLine($"Generated {KeySetSize:N0} station names with length from {minLen} to {maxLen}");
        return weatherStations;
    }

    private static void SkipComments(StreamReader reader)
    {
        while (reader.ReadLine().StartsWith("#"))
        {
        }
    }

    private static char ReadNonSpace(StringReader nameSource)
    {
        while (true)
        {
            int n = nameSource.Read();
            if (n == -1)
            {
                throw new IOException("Name source exhausted");
            }

            char ch = (char)n;

            if (ch != ' ')
            {
                return ch;
            }
        }
    }
}

public static class MathHelper
{

    public static double ToRadians(double angleIn10thofaDegree)
    {
        // Angle in 10th of a degree
        return (angleIn10thofaDegree * Math.PI) / 1800;
    }

}

public static class RandomExtensions
{
    private static bool hasDeviate;
    private static double storedDeviate;

    public static double NextGaussian(this Random random, double mean, double stddev)
    {
        if (hasDeviate)
        {
            hasDeviate = false;
            return mean + storedDeviate * stddev;
        }
        else
        {
            double u, v, s;
            do
            {
                u = 2 * random.NextDouble() - 1;
                v = 2 * random.NextDouble() - 1;
                s = u * u + v * v;
            } while (s >= 1 || s == 0);

            s = Math.Sqrt(-2 * Math.Log(s) / s);
            storedDeviate = v * s;
            hasDeviate = true;
            return mean + stddev * u * s;
        }
    }
}