using System.Collections;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace PortfolioManager.Dialogs;

public sealed partial class ExceptionDialog : Page
{
    public string ExceptionText { get; set; }
    public ExceptionDialog(Exception e)
    {
        InitializeComponent();

        var sb = new StringBuilder();

        sb.AppendLine(e.Message);

        if (e.Data.Count > 0)
        {
            sb.AppendLine("Extra details:");
            foreach (DictionaryEntry de in e.Data)
            {
                sb.Append($"{de.Value}");
            }
        }

        if (e.InnerException != null)
        {
            sb.AppendLine($"Inner exception: {e.InnerException}");
        }

        ExceptionText = sb.ToString();

        // Source
        // StackTrace
        // TargetSite
    }

}
