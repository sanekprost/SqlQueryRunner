using System.Data;

namespace SqlQueryRunner.Services;

public class ExcelExporter
{
    public void ExportToExcel(DataTable data, string filePath)
    {
        // Заглушка для экспорта в Excel
        // Будет реализована в Фазе 5
        throw new NotImplementedException("Экспорт в Excel будет реализован в Фазе 5");
    }

    public string? ShowSaveDialog()
    {
        // Заглушка для диалога сохранения
        // Будет реализована в Фазе 5
        using var saveDialog = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            FilterIndex = 1,
            DefaultExt = "xlsx",
            AddExtension = true,
            FileName = $"QueryResult_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            return saveDialog.FileName;
        }

        return null;
    }
}