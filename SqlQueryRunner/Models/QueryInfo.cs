namespace SqlQueryRunner.Models;

public class QueryInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public string SqlContent { get; set; } = string.Empty;
    public string SqlWithoutDeclares { get; set; } = string.Empty;
}