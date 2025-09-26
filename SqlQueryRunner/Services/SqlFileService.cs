using SqlQueryRunner.Models;

namespace SqlQueryRunner.Services;

public class SqlFileService
{
    private readonly DeclareBlockParser _parser;
    private readonly string _sqlFilesPath;

    public SqlFileService(string sqlFilesPath, DeclareBlockParser parser)
    {
        _sqlFilesPath = sqlFilesPath;
        _parser = parser;
    }

    public List<string> GetSqlFileNames()
    {
        try
        {
            if (!Directory.Exists(_sqlFilesPath))
            {
                return new List<string>();
            }

            return Directory.GetFiles(_sqlFilesPath, "*.sql")
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Cast<string>()
                .OrderBy(name => name)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при чтении папки с SQL файлами: {ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<string>();
        }
    }

    public QueryInfo? AnalyzeQuery(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(_sqlFilesPath, fileName);
            
            if (!File.Exists(fullPath))
            {
                return null;
            }

            var sqlContent = File.ReadAllText(fullPath);
            var parameters = _parser.ParseDeclareBlock(sqlContent);
            var sqlWithoutDeclares = _parser.RemoveDeclareBlock(sqlContent);

            return new QueryInfo
            {
                FileName = fileName,
                FullPath = fullPath,
                Parameters = parameters,
                SqlContent = sqlContent,
                SqlWithoutDeclares = sqlWithoutDeclares
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при анализе файла {fileName}: {ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    public void RefreshFileList()
    {
        // Метод для принудительного обновления списка файлов
        // Пока что пустая реализация, логика в GetSqlFileNames()
    }
}