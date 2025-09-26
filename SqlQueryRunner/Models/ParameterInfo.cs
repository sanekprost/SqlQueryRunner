using System;

namespace SqlQueryRunner.Models
{
    /// <summary>
    /// Представляет информацию о параметре SQL запроса
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Имя параметра (без @)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Человекочитаемое название для отображения в UI (из аннотации)
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Подробное описание параметра (из аннотации)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// SQL тип данных (например: DATE, INT, NVARCHAR(50))
        /// </summary>
        public string SqlType { get; set; } = string.Empty;

        /// <summary>
        /// Тип параметра для создания контролов
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        /// Значение по умолчанию из DECLARE
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Есть ли значение по умолчанию в DECLARE
        /// </summary>
        public bool HasDefault { get; set; }

        /// <summary>
        /// Обязателен ли параметр (обратное от HasDefault или NULL)
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Максимальная длина для строковых параметров
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Точность для DECIMAL параметров
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Шкала для DECIMAL параметров  
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Применяет аннотацию к параметру
        /// </summary>
        public void ApplyAnnotation(ParameterAnnotation annotation)
        {
            if (annotation == null || !annotation.ParameterName.Equals(Name, StringComparison.OrdinalIgnoreCase))
                return;

            DisplayName = annotation.DisplayName;
            Description = annotation.Description;
        }

        /// <summary>
        /// Получает отображаемое имя (аннотация или имя параметра)
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(DisplayName) ? Name : DisplayName;
        }

        /// <summary>
        /// Получает подсказку для контрола
        /// </summary>
        public string GetTooltip()
        {
            var tooltip = $"Параметр: @{Name}\nТип: {SqlType}";
            
            if (!string.IsNullOrWhiteSpace(Description))
                tooltip += $"\n\n{Description}";
                
            if (HasDefault)
                tooltip += $"\nПо умолчанию: {DefaultValue ?? "NULL"}";
                
            return tooltip;
        }

        /// <summary>
        /// Валидирует значение параметра
        /// </summary>
        public ValidationResult ValidateValue(object? value)
        {
            // Если параметр обязательный и значение пустое
            if (IsRequired && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                return new ValidationResult(false, $"Параметр '{GetDisplayName()}' обязателен для заполнения");
            }

            // Если значение null и это допустимо
            if (value == null)
                return ValidationResult.Success;

            // Валидация по типу
            switch (Type)
            {
                case ParameterType.String:
                    return ValidateStringValue(value);
                    
                case ParameterType.Integer:
                    return ValidateIntegerValue(value);
                    
                case ParameterType.Decimal:
                    return ValidateDecimalValue(value);
                    
                case ParameterType.DateTime:
                    return ValidateDateTimeValue(value);
                    
                case ParameterType.Boolean:
                    return ValidateBooleanValue(value);
                    
                default:
                    return ValidationResult.Success;
            }
        }

        private ValidationResult ValidateStringValue(object value)
        {
            var stringValue = value.ToString();
            if (MaxLength.HasValue && stringValue?.Length > MaxLength.Value)
            {
                return new ValidationResult(false, 
                    $"Длина параметра '{GetDisplayName()}' не должна превышать {MaxLength.Value} символов");
            }
            return ValidationResult.Success;
        }

        private ValidationResult ValidateIntegerValue(object value)
        {
            if (value is not int && !int.TryParse(value.ToString(), out _))
            {
                return new ValidationResult(false, 
                    $"Параметр '{GetDisplayName()}' должен быть целым числом");
            }
            return ValidationResult.Success;
        }

        private ValidationResult ValidateDecimalValue(object value)
        {
            if (value is not decimal && !decimal.TryParse(value.ToString(), out _))
            {
                return new ValidationResult(false, 
                    $"Параметр '{GetDisplayName()}' должен быть числом");
            }
            return ValidationResult.Success;
        }

        private ValidationResult ValidateDateTimeValue(object value)
        {
            if (value is not DateTime && !DateTime.TryParse(value.ToString(), out _))
            {
                return new ValidationResult(false, 
                    $"Параметр '{GetDisplayName()}' должен быть датой");
            }
            return ValidationResult.Success;
        }

        private ValidationResult ValidateBooleanValue(object value)
        {
            if (value is not bool && !bool.TryParse(value.ToString(), out _))
            {
                return new ValidationResult(false, 
                    $"Параметр '{GetDisplayName()}' должен быть логическим значением");
            }
            return ValidationResult.Success;
        }

        public override string ToString()
        {
            return $"{GetDisplayName()} (@{Name}, {SqlType})";
        }
    }
}