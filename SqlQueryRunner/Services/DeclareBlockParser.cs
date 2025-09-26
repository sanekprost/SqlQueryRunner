using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services
{
    /// <summary>
    /// Парсер DECLARE блоков с поддержкой аннотаций
    /// </summary>
    public class DeclareBlockParser
    {
        private static readonly Regex DeclareRegex = new Regex(
            @"DECLARE\s+@(\w+)\s+(\w+(?:\(\d+(?:,\s*\d+)?\))?)\s*=\s*(.+?)(?=\s*(?:DECLARE|$))",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );

        private static readonly Regex AnnotationRegex = new Regex(
            @"--\s*@param\s+(\w+)\s+""([^""]+)""\s*(?:""([^""]*)"")?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );

        /// <summary>
        /// Парсит SQL файл и извлекает параметры с аннотациями
        /// </summary>
        public List<ParameterInfo> ParseSqlFile(string sqlContent)
        {
            if (string.IsNullOrWhiteSpace(sqlContent))
                return new List<ParameterInfo>();

            // Извлекаем аннотации
            var annotations = ParseAnnotations(sqlContent);

            // Извлекаем параметры из DECLARE блока
            var parameters = ParseDeclareBlock(sqlContent);

            // Применяем аннотации к параметрам
            foreach (var parameter in parameters)
            {
                if (annotations.TryGetValue(parameter.Name.ToLowerInvariant(), out var annotation))
                {
                    parameter.ApplyAnnotation(annotation);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Парсит аннотации из комментариев
        /// </summary>
        private Dictionary<string, ParameterAnnotation> ParseAnnotations(string sqlContent)
        {
            var annotations = new Dictionary<string, ParameterAnnotation>(StringComparer.OrdinalIgnoreCase);
            var lines = sqlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var annotation = ParameterAnnotation.ParseFromLine(line.Trim());
                if (annotation != null)
                {
                    annotations[annotation.ParameterName] = annotation;
                }
            }

            return annotations;
        }

        /// <summary>
        /// Парсит DECLARE блок и извлекает параметры
        /// </summary>
        private List<ParameterInfo> ParseDeclareBlock(string sqlContent)
        {
            var parameters = new List<ParameterInfo>();
            var matches = DeclareRegex.Matches(sqlContent);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 4)
                {
                    var parameterName = match.Groups[1].Value;
                    var sqlType = match.Groups[2].Value.Trim();
                    var defaultValueStr = match.Groups[3].Value.Trim();

                    var parameter = CreateParameterInfo(parameterName, sqlType, defaultValueStr);
                    if (parameter != null)
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Создает ParameterInfo из распарсенных данных
        /// </summary>
        private ParameterInfo? CreateParameterInfo(string name, string sqlType, string defaultValueStr)
        {
            var parameter = new ParameterInfo
            {
                Name = name,
                SqlType = sqlType
            };

            // Определяем тип и парсим дефолтное значение
            var typeInfo = ParseSqlType(sqlType);
            parameter.Type = typeInfo.Type;
            parameter.MaxLength = typeInfo.MaxLength;
            parameter.Precision = typeInfo.Precision;
            parameter.Scale = typeInfo.Scale;

            // Парсим значение по умолчанию
            var defaultValue = ParseDefaultValue(defaultValueStr, parameter.Type);
            parameter.DefaultValue = defaultValue.Value;
            parameter.HasDefault = defaultValue.HasValue;
            parameter.IsRequired = !defaultValue.HasValue && !defaultValue.IsNull;

            return parameter;
        }

        /// <summary>
        /// Парсит SQL тип и извлекает дополнительную информацию
        /// </summary>
        private (ParameterType Type, int? MaxLength, int? Precision, int? Scale) ParseSqlType(string sqlType)
        {
            var upperType = sqlType.ToUpperInvariant();

            // Извлекаем параметры типа (например, NVARCHAR(50) или DECIMAL(10,2))
            var match = Regex.Match(upperType, @"(\w+)(?:\((\d+)(?:,\s*(\d+))?\))?");
            
            if (!match.Success)
                return (ParameterType.String, null, null, null);

            var baseType = match.Groups[1].Value;
            var param1 = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : (int?)null;
            var param2 = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : (int?)null;

            return baseType switch
            {
                "INT" or "INTEGER" or "SMALLINT" or "TINYINT" or "BIGINT" => 
                    (ParameterType.Integer, null, null, null),
                    
                "DECIMAL" or "NUMERIC" or "MONEY" or "SMALLMONEY" or "FLOAT" or "REAL" => 
                    (ParameterType.Decimal, null, param1, param2),
                    
                "DATE" or "DATETIME" or "DATETIME2" or "SMALLDATETIME" or "DATETIMEOFFSET" or "TIME" => 
                    (ParameterType.DateTime, null, null, null),
                    
                "BIT" => 
                    (ParameterType.Boolean, null, null, null),
                    
                "NVARCHAR" or "VARCHAR" or "CHAR" or "NCHAR" or "TEXT" or "NTEXT" => 
                    (ParameterType.String, param1 == -1 ? int.MaxValue : param1, null, null),
                    
                _ => (ParameterType.String, null, null, null)
            };
        }

        /// <summary>
        /// Парсит значение по умолчанию
        /// </summary>
        private (object? Value, bool HasValue, bool IsNull) ParseDefaultValue(string defaultStr, ParameterType type)
        {
            if (string.IsNullOrWhiteSpace(defaultStr))
                return (null, false, false);

            defaultStr = defaultStr.Trim();

            // Проверяем на NULL
            if (defaultStr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return (null, true, true);

            try
            {
                return type switch
                {
                    ParameterType.Integer => ParseIntegerDefault(defaultStr),
                    ParameterType.Decimal => ParseDecimalDefault(defaultStr),
                    ParameterType.DateTime => ParseDateTimeDefault(defaultStr),
                    ParameterType.Boolean => ParseBooleanDefault(defaultStr),
                    ParameterType.String => ParseStringDefault(defaultStr),
                    _ => (defaultStr, true, false)
                };
            }
            catch
            {
                // Если не удалось распарсить, возвращаем как строку
                return (defaultStr, true, false);
            }
        }

        private (object?, bool, bool) ParseIntegerDefault(string defaultStr)
        {
            if (int.TryParse(defaultStr, out var intValue))
                return (intValue, true, false);
            return (defaultStr, true, false);
        }

        private (object?, bool, bool) ParseDecimalDefault(string defaultStr)
        {
            if (decimal.TryParse(defaultStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var decValue))
                return (decValue, true, false);
            return (defaultStr, true, false);
        }

        private (object?, bool, bool) ParseDateTimeDefault(string defaultStr)
        {
            // Убираем кавычки если есть
            defaultStr = defaultStr.Trim('\'', '"');
            
            if (DateTime.TryParse(defaultStr, out var dateValue))
                return (dateValue, true, false);
            return (defaultStr, true, false);
        }

        private (object?, bool, bool) ParseBooleanDefault(string defaultStr)
        {
            if (defaultStr.Equals("1", StringComparison.OrdinalIgnoreCase) || 
                defaultStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                return (true, true, false);
                
            if (defaultStr.Equals("0", StringComparison.OrdinalIgnoreCase) || 
                defaultStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                return (false, true, false);
                
            return (defaultStr, true, false);
        }

        private (object?, bool, bool) ParseStringDefault(string defaultStr)
        {
            // Убираем кавычки если есть
            if ((defaultStr.StartsWith("'") && defaultStr.EndsWith("'")) ||
                (defaultStr.StartsWith("\"") && defaultStr.EndsWith("\"")))
            {
                defaultStr = defaultStr.Substring(1, defaultStr.Length - 2);
            }
            
            return (defaultStr, true, false);
        }

        /// <summary>
        /// Удаляет DECLARE блок из SQL запроса для выполнения
        /// </summary>
        public string RemoveDeclareBlock(string sqlContent)
        {
            if (string.IsNullOrWhiteSpace(sqlContent))
                return string.Empty;

            var lines = sqlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var resultLines = new List<string>();
            bool inDeclareBlock = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Пропускаем аннотации
                if (trimmedLine.StartsWith("-- @param", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                // Проверяем начало DECLARE блока
                if (trimmedLine.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase))
                {
                    inDeclareBlock = true;
                    continue;
                }

                // Если в DECLARE блоке, проверяем его завершение
                if (inDeclareBlock)
                {
                    // DECLARE блок заканчивается, когда встречаем SQL команду
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && 
                        !trimmedLine.StartsWith("--") &&
                        !trimmedLine.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase))
                    {
                        inDeclareBlock = false;
                        resultLines.Add(line);
                    }
                }
                else
                {
                    resultLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, resultLines);
        }
    }
}