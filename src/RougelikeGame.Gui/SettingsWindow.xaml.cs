using System.Windows;
using System.Windows.Input;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 設定画面ウィンドウ
/// </summary>
public partial class SettingsWindow : Window
{
    private GameSettings _settings;
    private bool _isInitializing = true;

    public SettingsWindow()
    {
        InitializeComponent();

        _settings = GameSettings.Load();
        LoadSettingsToUI();
        _isInitializing = false;
    }

    private void LoadSettingsToUI()
    {
        MasterVolumeSlider.Value = _settings.MasterVolume * 100;
        BgmVolumeSlider.Value = _settings.BgmVolume * 100;
        SeVolumeSlider.Value = _settings.SeVolume * 100;
        FontSizeSlider.Value = _settings.FontSize;

        UpdateVolumeLabels();
        UpdateFontSizeLabel();
    }

    private void UpdateVolumeLabels()
    {
        MasterVolumeText.Text = $"{(int)MasterVolumeSlider.Value}%";
        BgmVolumeText.Text = $"{(int)BgmVolumeSlider.Value}%";
        SeVolumeText.Text = $"{(int)SeVolumeSlider.Value}%";
    }

    private void UpdateFontSizeLabel()
    {
        FontSizeText.Text = $"{(int)FontSizeSlider.Value}";
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;
        UpdateVolumeLabels();
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing) return;
        UpdateFontSizeLabel();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.MasterVolume = (float)(MasterVolumeSlider.Value / 100.0);
        _settings.BgmVolume = (float)(BgmVolumeSlider.Value / 100.0);
        _settings.SeVolume = (float)(SeVolumeSlider.Value / 100.0);
        _settings.FontSize = (int)FontSizeSlider.Value;

        _settings.Save();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _settings = GameSettings.CreateDefault();
        _isInitializing = true;
        LoadSettingsToUI();
        _isInitializing = false;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }
}
