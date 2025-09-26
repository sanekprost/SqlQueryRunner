using System.Text;

namespace SqlQueryRunner.Models;

/// <summary>
/// Представляет аннотацию для параметра SQL запроса
/// </summary>
public class ParameterAnnotation
{
    /// <summary>
    /// Имя параметра (без @)
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Человекочитаемое название для отображения в UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Подробное описание параметра
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Проверяет, является ли аннотация валидной
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(ParameterName) && !string.IsNullOrWhiteSpace(DisplayName);

    /// <summary>
    /// Создает аннотацию из строки формата: @param ParamName "DisplayName" "Description"
    /// </summary>
    public static ParameterAnnotation? ParseFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        line = line.Trim();

        // Проверяем, что строка начинается с -- @param
        if (!line.StartsWith("-- @param ", StringComparison.OrdinalIgnoreCase))
            return null;

        // Убираем префикс
        var content = line.Substring("-- @param ".Length).Trim();

        // Парсим параметры: ParamName "DisplayName" "Description"
        var parts = ParseQuotedString(content);

        if (parts.Length < 2)
            return null;

        var annotation = new ParameterAnnotation
        {
            ParameterName = parts[0].Trim(),
            DisplayName = parts[1]
        };

        // Описание опционально
        if (parts.Length > 2)
        {
            annotation.Description = parts[2];
        }

        return annotation.IsValid ? annotation : null;
    }

    /// <summary>
    /// Парсит строку с кавычками: ParamName "Display Name" "Description"
    /// </summary>
    private static string[] ParseQuotedString(string input)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        bool firstParam = true;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                if (!inQuotes && current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    firstParam = false;
                }
            }
            else if (c == ' ' && !inQuotes)
            {
                if (firstParam && current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    firstParam = false;
                }
                // Игнорируем пробелы вне кавычек между параметрами
            }
            else
            {
                current.Append(c);
            }
        }

        // Добавляем последний параметр если он есть
        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result.ToArray();
    }

    public override string ToString()
    {
        return $"@{ParameterName}: {DisplayName}" +
               (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");
    }
}