using System.Text.Json;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services;

public class ConfigurationService
{
    private const string ConfigFileName = "appsettings.json";
    private AppSettings? _settings;

    public AppSettings LoadSettings()
    {
        if (_settings != null)
            return _settings;

        try
        {
            if (File.Exists(ConfigFileName))
            {
                var json = File.ReadAllText(ConfigFileName);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? GetDefaultSettings();
            }
            else
            {
                _settings = GetDefaultSettings();
                SaveSettings(_settings);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}\nИспользуются настройки по умолчанию.",
                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _settings = GetDefaultSettings();
        }

        // Создаем необходимые папки
        EnsureDirectoriesExist();
        
        return _settings;
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(ConfigFileName, json);
            _settings = settings;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения конфигурации: {ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public AppSettings GetDefaultSettings()
    {
        return new AppSettings();
    }

    private void EnsureDirectoriesExist()
    {
        if (_settings == null) return;

        try
        {
            if (!Directory.Exists(_settings.SqlFilesPath))
            {
                Directory.CreateDirectory(_settings.SqlFilesPath);
            }

            if (!Directory.Exists(_settings.DefaultExportPath))
            {
                Directory.CreateDirectory(_settings.DefaultExportPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка создания папок: {ex.Message}",
                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public void RefreshSettings()
    {
        _settings = null;
        LoadSettings();
    }
}