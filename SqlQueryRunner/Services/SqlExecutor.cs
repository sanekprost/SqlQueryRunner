using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services
{
    /// <summary>
    /// Сервис для выполнения SQL запросов (заглушка для Фазы 3)
    /// </summary>
    public class SqlExecutor
    {
        private readonly string _connectionString;
        private readonly int _queryTimeout;

        public SqlExecutor()
        {
            // Заглушка для Фазы 3 - конструктор без параметров
            _connectionString = "";
            _queryTimeout = 30;
        }

        public SqlExecutor(string connectionString, int queryTimeout = 30)
        {
            _connectionString = connectionString;
            _queryTimeout = queryTimeout;
        }

        public async Task<bool> TestConnectionAsync()
        {
            // Заглушка - в Фазе 4 будет реальная проверка
            return await Task.FromResult(false);
        }

        public async Task<(DataTable? Result, string? ErrorMessage, TimeSpan ExecutionTime)> ExecuteQueryAsync(
            string sql, Dictionary<string, object?> parameters)
        {
            // Заглушка - в Фазе 4 будет реальное выполнение
            return await Task.FromResult<(DataTable?, string?, TimeSpan)>((null, "Не реализовано в Фазе 3", TimeSpan.Zero));
        }

        public Dictionary<string, object?> ConvertParametersForSql(List<ParameterInfo> parameterInfos, 
            Dictionary<string, object?> userValues)
        {
            var sqlParameters = new Dictionary<string, object?>();

            foreach (var paramInfo in parameterInfos)
            {
                if (userValues.TryGetValue(paramInfo.Name, out var userValue))
                {
                    sqlParameters[paramInfo.Name] = ConvertValueForSql(userValue, paramInfo.Type);
                }
                else if (paramInfo.HasDefault)
                {
                    sqlParameters[paramInfo.Name] = ConvertValueForSql(paramInfo.DefaultValue, paramInfo.Type);
                }
                else
                {
                    sqlParameters[paramInfo.Name] = DBNull.Value;
                }
            }

            return sqlParameters;
        }

        private object? ConvertValueForSql(object? value, ParameterType type)
        {
            if (value == null)
                return DBNull.Value;

            return type switch
            {
                ParameterType.DateTime => value is DateTime dt ? dt : value,
                ParameterType.Integer => Convert.ToInt32(value),
                ParameterType.Decimal => Convert.ToDecimal(value),
                ParameterType.Boolean => Convert.ToBoolean(value),
                ParameterType.String => value.ToString(),
                _ => value
            };
        }
    }
}