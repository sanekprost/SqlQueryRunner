// namespace SqlQueryRunner.Models;
//
// public enum ParameterType
// {
//     Date,
//     DateTime,
//     Int,
//     Decimal,
//     NVarchar,
//     Varchar,
//     Bit
// }
//
// public class ParameterInfo
// {
//     public string Name { get; set; } = string.Empty;
//     public string SqlType { get; set; } = string.Empty;
//     public ParameterType Type { get; set; }
//     public object? DefaultValue { get; set; }
//     public bool HasDefault { get; set; }
//     public bool IsRequired { get; set; }
// }
//
// public class QueryInfo
// {
//     public string FileName { get; set; } = string.Empty;
//     public string FullPath { get; set; } = string.Empty;
//     public List<ParameterInfo> Parameters { get; set; } = new();
//     public string SqlContent { get; set; } = string.Empty;
//     public string SqlWithoutDeclares { get; set; } = string.Empty;
// }