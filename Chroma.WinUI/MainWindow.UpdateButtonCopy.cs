using Microsoft.UI.Xaml;

namespace Chroma;

public sealed partial class MainWindow
{
    public void EnableUpdateButtonCopy()
    {
        SetIdleUpdateButtonText();
        UpdatesButton.IsEnabledChanged += UpdatesButton_IsEnabledChanged;
    }

    private void UpdatesButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        if (UpdatesButton.IsEnabled)
        {
            SetIdleUpdateButtonText();
        }
    }

    private void SetIdleUpdateButtonText()
    {
        if (UpdatesButtonText.Text is "Updates" or "Up to date" or "Check for updates")
        {
            UpdatesButtonText.Text = "Check updates";
        }
    }
}
