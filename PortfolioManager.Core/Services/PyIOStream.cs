using System.Text;

namespace PortfolioManager.Core.Services;

/// <summary>
/// Implement the interface of the sys.stdout redirection
/// </summary>
public class PyIOStream
{
    private readonly StringBuilder TextBuilder;
    private readonly StringWriter TextWriter;

    public PyIOStream(StringBuilder builder = null)
    {
        TextBuilder = builder ?? new StringBuilder();
        TextWriter = new StringWriter(TextBuilder);
    }

    public event EventHandler<string> OnWriteUpdate;

    public void ClearBuffer()
    {
        TextBuilder.Clear();
    }

    public string GetBuffer()
    {
        return TextBuilder.ToString();
    }

    public void write(string str)
    {
        TextWriter.Write(str);
        OnWriteUpdate?.Invoke(this, str);
    }

    public void writelines(IEnumerable<string> str)
    {
        foreach (var line in str)
        {
            write(line);
        }
    }

    public void flush()
    {
        TextWriter.Flush();
    }

    public void close()
    {
        TextWriter?.Close();
    }
}
