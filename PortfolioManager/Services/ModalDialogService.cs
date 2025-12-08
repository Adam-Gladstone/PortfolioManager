using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PortfolioManager.Services;

public static class ModalDialog
{
    public static async Task MessageDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string message
        )
    {
        await MessageDialogAsync(element, title, message, "OK");
    }

    public static async Task MessageDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string message, 
        string buttonText
        )
    {
        var dialog = new ContentDialog
        {
            XamlRoot = element?.XamlRoot,
            RequestedTheme = element?.ActualTheme ?? ElementTheme.Default,
            Title = title,
            CloseButtonText = buttonText,
            Content = message,
        };

        await dialog.ShowAsync();
    }

    public static async Task<bool?> ConfirmationDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string message
        )
    {
        return await ConfirmationDialogAsync(element, title, message, "OK", string.Empty, "Cancel");
    }

    public static async Task<bool> ConfirmationDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string message, 
        string yesButtonText, 
        string noButtonText
        )
    {
        return (await ConfirmationDialogAsync(element, title, message, yesButtonText, noButtonText, string.Empty)).Value;
    }


    public static async Task<bool?> ConfirmationDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string message, 
        string yesButtonText, 
        string noButtonText, 
        string cancelButtonText
        )
    {
        var dialog = new ContentDialog
        {
            XamlRoot = element?.XamlRoot,
            RequestedTheme = element?.ActualTheme ?? ElementTheme.Default,
            Title = title,
            PrimaryButtonText = yesButtonText,
            SecondaryButtonText = noButtonText,
            CloseButtonText = cancelButtonText,
            DefaultButton = ContentDialogButton.Primary,
            Content = message,
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.None)
        {
            return null;
        }

        return (result == ContentDialogResult.Primary);
    }

    public static async Task<string> InputStringDialogAsync(
        this FrameworkElement? element, 
        string title
        )
    {
        return await element.InputStringDialogAsync(title, string.Empty);
    }

    public static async Task<string> InputStringDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string defaultText
        )
    {
        return await element.InputStringDialogAsync(title, defaultText, "OK", "Cancel");
    }

    public static async Task<string> InputStringDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string defaultText, 
        string okButtonText, 
        string cancelButtonText
        )
    {
        var inputTextBox = new TextBox
        {
            AcceptsReturn = false,
            Height = 32,
            Text = defaultText,
            SelectionStart = defaultText.Length
        };
        
        var dialog = new ContentDialog
        {
            XamlRoot = element?.XamlRoot,
            RequestedTheme = element?.ActualTheme ?? ElementTheme.Default,
            Content = inputTextBox,
            Title = title,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = okButtonText,
            SecondaryButtonText = cancelButtonText,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            return inputTextBox.Text;
        }
        else
        {
            return string.Empty;
        }
    }

    public static async Task<string> InputTextDialogAsync(
        this FrameworkElement? element, 
        string title
        )
    {
        return await element.InputTextDialogAsync(title, string.Empty);
    }

    public static async Task<string> InputTextDialogAsync(
        this FrameworkElement? element, 
        string title, 
        string defaultText
        )
    {
        var inputTextBox = new TextBox
        {
            AcceptsReturn = true,
            Height = 32 * 6,
            Text = defaultText,
            TextWrapping = TextWrapping.Wrap,
            SelectionStart = defaultText.Length
        };

        var dialog = new ContentDialog
        {
            XamlRoot = element?.XamlRoot,
            RequestedTheme = element?.ActualTheme ?? ElementTheme.Default,
            Title = title,
            Content = inputTextBox,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Ok",
            SecondaryButtonText = "Cancel",
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            return inputTextBox.Text;
        }
        else
        {
            return string.Empty;
        }
    }
}
