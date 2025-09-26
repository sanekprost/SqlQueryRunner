using System.Collections.Generic;

namespace SqlQueryRunner.Models
{
    /// <summary>
    /// Информация о SQL запросе
    /// </summary>
    public class QueryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public List<ParameterInfo> Parameters { get; set; } = new();
        public int ParametersCount { get; set; }
        public bool HasAnnotations { get; set; }
        public string? Error { get; set; }
        public string? SqlContent { get; set; }
        public string? SqlWithoutDeclares { get; set; }
        public override string ToString() => DisplayName;
    }
}