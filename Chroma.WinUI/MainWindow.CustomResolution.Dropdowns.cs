using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private ComboBox? _resolutionWidthDropDown;
    private ComboBox? _resolutionHeightDropDown;
    private bool _syncingResolutionDropDowns;

    private static readonly IReadOnlyDictionary<int, int[]> CommonResolutionModes =
        new Dictionary<int, int[]>
        {
            [1024] = [768],
            [1152] = [864],
            [1280] = [720, 800, 960, 1024],
            [1360] = [768],
            [1366] = [768],
            [1440] = [900, 1080],
            [1600] = [900, 1024, 1200],
            [1680] = [1050],
            [1728] = [1080],
            [1920] = [1080, 1200],
            [2048] = [1152],
            [2560] = [1080, 1440, 1600],
            [3440] = [1440],
            [3840] = [2160]
        };

    public void EnableCustomResolutionDropdowns()
    {
        if (_resolutionWidthTextBox is null || _resolutionHeightTextBox is null)
        {
            return;
        }

        StackPanel? widthField = VisualTreeHelper.GetParent(_resolutionWidthTextBox) as StackPanel;
        StackPanel? heightField = VisualTreeHelper.GetParent(_resolutionHeightTextBox) as StackPanel;
        if (widthField is null || heightField is null)
        {
            return;
        }

        _resolutionWidthDropDown = CreateResolutionDropDown("Select width");
        _resolutionHeightDropDown = CreateResolutionDropDown("Select height");

        widthField.Children.Remove(_resolutionWidthTextBox);
        heightField.Children.Remove(_resolutionHeightTextBox);
        widthField.Children.Add(_resolutionWidthDropDown);
        heightField.Children.Add(_resolutionHeightDropDown);

        _resolutionWidthDropDown.SelectionChanged += ResolutionWidthDropDown_SelectionChanged;
        _resolutionHeightDropDown.SelectionChanged += ResolutionHeightDropDown_SelectionChanged;
        _customResolutionToggle!.Toggled += (_, _) => UpdateResolutionDropDownState();
        ProfilesList.SelectionChanged += (_, _) => DispatcherQueue.TryEnqueue(SyncResolutionDropDownsFromBackingFields);

        SyncResolutionDropDownsFromBackingFields();
    }

    private static ComboBox CreateResolutionDropDown(string placeholder)
    {
        return new ComboBox
        {
            Height = 42,
            PlaceholderText = placeholder,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            MaxDropDownHeight = 320
        };
    }

    private void SyncResolutionDropDownsFromBackingFields()
    {
        if (_resolutionWidthDropDown is null || _resolutionHeightDropDown is null ||
            _resolutionWidthTextBox is null || _resolutionHeightTextBox is null)
        {
            return;
        }

        _syncingResolutionDropDowns = true;
        try
        {
            int.TryParse(_resolutionWidthTextBox.Text, out int selectedWidth);
            int.TryParse(_resolutionHeightTextBox.Text, out int selectedHeight);

            List<int> widths = CommonResolutionModes.Keys.OrderBy(value => value).ToList();
            if (selectedWidth > 0 && !widths.Contains(selectedWidth))
            {
                widths.Add(selectedWidth);
                widths.Sort();
            }

            _resolutionWidthDropDown.ItemsSource = widths;
            _resolutionWidthDropDown.SelectedItem = selectedWidth > 0 ? selectedWidth : 1920;
            PopulateHeightDropDown(selectedWidth > 0 ? selectedWidth : 1920, selectedHeight);
        }
        finally
        {
            _syncingResolutionDropDowns = false;
        }

        UpdateResolutionDropDownState();
    }

    private void ResolutionWidthDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingResolutionDropDowns || _resolutionWidthDropDown?.SelectedItem is not int width)
        {
            return;
        }

        _syncingResolutionDropDowns = true;
        try
        {
            int previousHeight = _resolutionHeightDropDown?.SelectedItem as int? ?? 0;
            _resolutionWidthTextBox!.Text = width.ToString();
            PopulateHeightDropDown(width, previousHeight);
            if (_resolutionHeightDropDown?.SelectedItem is int height)
            {
                _resolutionHeightTextBox!.Text = height.ToString();
            }
        }
        finally
        {
            _syncingResolutionDropDowns = false;
        }

        UpdateResolutionEditorState();
    }

    private void ResolutionHeightDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingResolutionDropDowns || _resolutionHeightDropDown?.SelectedItem is not int height)
        {
            return;
        }

        _resolutionHeightTextBox!.Text = height.ToString();
        UpdateResolutionEditorState();
    }

    private void PopulateHeightDropDown(int width, int preferredHeight)
    {
        if (_resolutionHeightDropDown is null)
        {
            return;
        }

        List<int> heights = CommonResolutionModes.TryGetValue(width, out int[]? supported)
            ? supported.OrderBy(value => value).ToList()
            : new List<int>();

        if (preferredHeight > 0 && !heights.Contains(preferredHeight))
        {
            heights.Add(preferredHeight);
            heights.Sort();
        }

        if (heights.Count == 0)
        {
            heights.Add(1080);
        }

        _resolutionHeightDropDown.ItemsSource = heights;
        _resolutionHeightDropDown.SelectedItem = heights.Contains(preferredHeight)
            ? preferredHeight
            : heights[^1];
    }

    private void UpdateResolutionDropDownState()
    {
        bool enabled = _customResolutionToggle?.IsEnabled == true && _customResolutionToggle.IsOn;
        if (_resolutionWidthDropDown is not null)
        {
            _resolutionWidthDropDown.IsEnabled = enabled;
        }
        if (_resolutionHeightDropDown is not null)
        {
            _resolutionHeightDropDown.IsEnabled = enabled;
        }
    }
}
