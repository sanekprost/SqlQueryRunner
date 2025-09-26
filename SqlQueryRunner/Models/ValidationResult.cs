namespace SqlQueryRunner.Models;

/// <summary>
/// Результат валидации параметра
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    public ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success => new(true);
    public static ValidationResult Error(string message) => new(false, message);
}