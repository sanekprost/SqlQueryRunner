using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services;

public class SqlExecutor
{
    private readonly string _connectionString;
    private readonly int _queryTimeout;

    public SqlExecutor(string connectionString, int queryTimeout = 30)
    {
        _connectionString = connectionString;
        _queryTimeout = queryTimeout;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(DataTable? Result, string? ErrorMessage, TimeSpan ExecutionTime)> ExecuteQueryAsync(
        string sql, Dictionary<string, object?> parameters)
    {
        var startTime = DateTime.Now;
        
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var dynamicParameters = new DynamicParameters();
            foreach (var param in parameters)
            {
                dynamicParameters.Add($"@{param.Key}", param.Value);
            }

            using var reader = await connection.ExecuteReaderAsync(sql, dynamicParameters, 
                commandTimeout: _queryTimeout);
            
            var dataTable = new DataTable();
            dataTable.Load(reader);
            
            var executionTime = DateTime.Now - startTime;
            
            return (dataTable, null, executionTime);
        }
        catch (Exception ex)
        {
            var executionTime = DateTime.Now - startTime;
            return (null, ex.Message, executionTime);
        }
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
            ParameterType.Date => value is DateTime dt ? dt.Date : value,
            ParameterType.DateTime => value,
            ParameterType.Int => Convert.ToInt32(value),
            ParameterType.Decimal => Convert.ToDecimal(value),
            ParameterType.Bit => Convert.ToBoolean(value),
            ParameterType.NVarchar or ParameterType.Varchar => value.ToString(),
            _ => value
        };
    }
}