using Avalonia.Controls;
using Avalonia.Interactivity;
using BabySmash.Linux.Core.Interfaces;
using BabySmash.Linux.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BabySmash.Linux;

public partial class OptionsWindow : Window
{
    private readonly ISettingsService _settingsService;

    public OptionsWindow()
    {
        InitializeComponent();
        
        _settingsService = App.Services.GetRequiredService<ISettingsService>();
        
        LoadSettings();
        
        // Wire up buttons
        var saveButton = this.FindControl<Button>("SaveButton");
        var cancelButton = this.FindControl<Button>("CancelButton");
        
        if (saveButton != null) saveButton.Click += OnSaveClick;
        if (cancelButton != null) cancelButton.Click += OnCancelClick;
    }

    private void LoadSettings()
    {
        // Sound mode
        var soundMode = _settingsService.Get(SettingsKeys.Sounds, SettingsDefaults.Sounds);
        var soundModeCombo = this.FindControl<ComboBox>("SoundModeCombo");
        if (soundModeCombo != null)
        {
            for (int i = 0; i < soundModeCombo.Items.Count; i++)
            {
                if (soundModeCombo.Items[i] is ComboBoxItem item && item.Tag?.ToString() == soundMode)
                {
                    soundModeCombo.SelectedIndex = i;
                    break;
                }
            }
        }
        
        // Checkboxes
        SetCheckbox("FacesOnShapesCheck", _settingsService.Get(SettingsKeys.FacesOnShapes, SettingsDefaults.FacesOnShapes));
        SetCheckbox("FadeAwayCheck", _settingsService.Get(SettingsKeys.FadeAway, SettingsDefaults.FadeAway));
        SetCheckbox("MouseDrawCheck", _settingsService.Get(SettingsKeys.MouseDraw, SettingsDefaults.MouseDraw));
        
        // Clear after
        var clearAfterNumeric = this.FindControl<NumericUpDown>("ClearAfterNumeric");
        if (clearAfterNumeric != null)
        {
            clearAfterNumeric.Value = _settingsService.Get(SettingsKeys.ClearAfter, SettingsDefaults.ClearAfter);
        }
    }

    private void SetCheckbox(string name, bool value)
    {
        var checkbox = this.FindControl<CheckBox>(name);
        if (checkbox != null)
        {
            checkbox.IsChecked = value;
        }
    }

    private bool GetCheckbox(string name)
    {
        var checkbox = this.FindControl<CheckBox>(name);
        return checkbox?.IsChecked ?? false;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        // Sound mode
        var soundModeCombo = this.FindControl<ComboBox>("SoundModeCombo");
        if (soundModeCombo?.SelectedItem is ComboBoxItem selectedItem)
        {
            _settingsService.Set(SettingsKeys.Sounds, selectedItem.Tag?.ToString() ?? "Speech");
        }
        
        // Checkboxes
        _settingsService.Set(SettingsKeys.FacesOnShapes, GetCheckbox("FacesOnShapesCheck"));
        _settingsService.Set(SettingsKeys.FadeAway, GetCheckbox("FadeAwayCheck"));
        _settingsService.Set(SettingsKeys.MouseDraw, GetCheckbox("MouseDrawCheck"));
        
        // Clear after
        var clearAfterNumeric = this.FindControl<NumericUpDown>("ClearAfterNumeric");
        if (clearAfterNumeric != null)
        {
            _settingsService.Set(SettingsKeys.ClearAfter, (int)(clearAfterNumeric.Value ?? 30));
        }
        
        _settingsService.Save();
        
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
