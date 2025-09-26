using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services
{
    /// <summary>
    /// Сервис для работы с SQL файлами
    /// </summary>
    public class SqlFileService
    {
        private readonly DeclareBlockParser _parser;

        public SqlFileService()
        {
            _parser = new DeclareBlockParser();
        }

        public SqlFileService(DeclareBlockParser parser)
        {
            _parser = parser ?? new DeclareBlockParser();
        }

        /// <summary>
        /// Получает список всех .sql файлов из указанной папки
        /// </summary>
        public List<string> GetSqlFiles(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Путь к папке не может быть пустым", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                return new List<string>();

            try
            {
                return Directory.GetFiles(directoryPath, "*.sql", SearchOption.TopDirectoryOnly)
                    .OrderBy(Path.GetFileName)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при чтении SQL файлов из папки {directoryPath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Читает содержимое SQL файла
        /// </summary>
        public string ReadSqlFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SQL файл не найден: {filePath}");

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при чтении файла {filePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Анализирует SQL файл и извлекает информацию о параметрах
        /// </summary>
        public QueryInfo? AnalyzeQuery(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var sqlContent = ReadSqlFile(filePath);
                var parameters = _parser.ParseSqlFile(sqlContent);
                var sqlWithoutDeclares = _parser.RemoveDeclareBlock(sqlContent);

                return new QueryInfo
                {
                    Name = fileName,
                    FilePath = filePath,
                    DisplayName = fileName.Replace("_", " "),
                    Parameters = parameters,
                    SqlContent = sqlContent,
                    SqlWithoutDeclares = sqlWithoutDeclares,
                    ParametersCount = parameters.Count,
                    HasAnnotations = parameters.Any(p => !string.IsNullOrEmpty(p.DisplayName))
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при анализе файла {filePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Проверяет, является ли файл валидным SQL файлом
        /// </summary>
        public bool IsValidSqlFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            if (!filePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var content = File.ReadAllText(filePath);
                return !string.IsNullOrWhiteSpace(content);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает список имен SQL файлов (только имена файлов)
        /// </summary>
        public List<string> GetSqlFileNames(string directoryPath)
        {
            var fullPaths = GetSqlFiles(directoryPath);
            return fullPaths.Select(Path.GetFileName)
                           .Where(name => !string.IsNullOrEmpty(name))
                           .Cast<string>()
                           .ToList();
        }
    }
}