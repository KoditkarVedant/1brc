namespace OneBRC;

public interface ISolution : IDisposable
{
    void Process(string filePath);
}