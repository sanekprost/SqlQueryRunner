using System;
using System.IO;
using System.Text.Json;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services
{
    /// <summary>
    /// Упрощенный сервис для работы с настройками
    /// </summary>
    public class ConfigurationService
    {
        private const string ConfigFileName = "appsettings.json";
        
        /// <summary>
        /// Загружает настройки из файла конфигурации
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                {
                    var defaultSettings = GetDefaultSettings();
                    SaveSettings(defaultSettings);
                    return defaultSettings;
                }

                var json = File.ReadAllText(ConfigFileName);
                
                // Простой парсинг JSON для базовых настроек
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var settings = JsonSerializer.Deserialize<AppSettings>(json, options) ?? GetDefaultSettings();
                
                // Создаем папки если нужно
                EnsureDirectoriesExist(settings);
                
                return settings;
            }
            catch (Exception)
            {
                // При ошибке возвращаем настройки по умолчанию
                return GetDefaultSettings();
            }
        }

        /// <summary>
        /// Сохраняет настройки в файл конфигурации
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                if (settings == null)
                    return;

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(ConfigFileName, json);
            }
            catch
            {
                // Игнорируем ошибки сохранения в упрощенной версии
            }
        }

        /// <summary>
        /// Возвращает настройки по умолчанию
        /// </summary>
        public AppSettings GetDefaultSettings()
        {
            return new AppSettings
            {
                ConnectionString = "Server=.;Database=TestDB;Integrated Security=true;TrustServerCertificate=true;",
                SqlFilesPath = "./SqlQueries",
                DefaultExportPath = "./Exports",
                AutoRefreshFiles = true,
                QueryTimeout = 30
            };
        }

        /// <summary>
        /// Проверяет существование папок и создает их при необходимости
        /// </summary>
        private void EnsureDirectoriesExist(AppSettings settings)
        {
            try
            {
                if (!Directory.Exists(settings.SqlFilesPath))
                {
                    Directory.CreateDirectory(settings.SqlFilesPath);
                }

                if (!Directory.Exists(settings.DefaultExportPath))
                {
                    Directory.CreateDirectory(settings.DefaultExportPath);
                }
            }
            catch
            {
                // Игнорируем ошибки создания папок
            }
        }
    }
}