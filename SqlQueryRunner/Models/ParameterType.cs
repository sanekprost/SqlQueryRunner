namespace SqlQueryRunner.Models
{
    /// <summary>
    /// Типы параметров SQL запросов
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Строковый тип (VARCHAR, NVARCHAR, etc.)
        /// </summary>
        String = 0,

        /// <summary>
        /// Целочисленный тип (INT, BIGINT, etc.)
        /// </summary>
        Integer = 1,

        /// <summary>
        /// Десятичный тип (DECIMAL, NUMERIC, MONEY, etc.)
        /// </summary>
        Decimal = 2,

        /// <summary>
        /// Тип даты и времени (DATE, DATETIME, etc.)
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// Логический тип (BIT)
        /// </summary>
        Boolean = 4
    }
}