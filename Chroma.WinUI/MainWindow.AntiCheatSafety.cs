using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chroma;

public sealed partial class MainWindow
{
    private Border? _antiCheatSafetyCard;

    public void EnableAntiCheatSafety()
    {
        if (_antiCheatSafetyCard is not null || AboutPage.Content is not StackPanel aboutStack)
        {
            return;
        }

        var heading = new TextBlock
        {
            Text = "Anti-cheat safety",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CyanBrush"],
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };

        var statusBadge = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(11, 5, 11, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                Text = "LOW-RISK BY DESIGN",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PositiveBrush"],
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                CharacterSpacing = 90
            }
        };

        var summary = new TextBlock
        {
            Text = "Chroma operates outside game processes and changes display saturation through GPU-vendor control APIs. Its current architecture avoids the techniques commonly associated with cheats.",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        };

        var details = new StackPanel { Spacing = 7, Margin = new Thickness(0, 3, 0, 0) };
        details.Children.Add(CreateSafetyLine("No DLL injection, game-code hooks, or internal overlay"));
        details.Children.Add(CreateSafetyLine("No reading or writing of game memory"));
        details.Children.Add(CreateSafetyLine("No game-file modification, input automation, or kernel driver"));
        details.Children.Add(CreateSafetyLine("Foreground detection uses a limited Windows process-name query only"));
        details.Children.Add(CreateSafetyLine("Color control uses Intel IGCL, NVIDIA NVAPI, or AMD ADLX"));

        var assessment = new TextBlock
        {
            Text = "Based on the current source code, Chroma is considered low risk for VAC, BattlEye, Easy Anti-Cheat, and Riot Vanguard. This is an architectural assessment, not an official certification or allowlisting.",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 13.5,
            TextWrapping = TextWrapping.Wrap
        };

        var disclaimer = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 11, 14, 11),
            Margin = new Thickness(0, 3, 0, 0),
            Child = new TextBlock
            {
                Text = "Important: No third-party utility can guarantee approval by every game or anti-cheat provider. Publisher policies and detection rules can change. Stop using Chroma with a game if its publisher explicitly prohibits external color-adjustment utilities.",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextMutedBrush"],
                FontSize = 12.5,
                TextWrapping = TextWrapping.Wrap
            }
        };

        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(heading);
        content.Children.Add(statusBadge);
        content.Children.Add(summary);
        content.Children.Add(details);
        content.Children.Add(assessment);
        content.Children.Add(disclaimer);

        _antiCheatSafetyCard = new Border
        {
            Style = (Style)Application.Current.Resources["CardBorderStyle"],
            Padding = new Thickness(24),
            Child = content
        };

        aboutStack.Children.Add(_antiCheatSafetyCard);
    }

    private static TextBlock CreateSafetyLine(string text)
    {
        return new TextBlock
        {
            Text = $"• {text}",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 13.5,
            TextWrapping = TextWrapping.Wrap
        };
    }
}
