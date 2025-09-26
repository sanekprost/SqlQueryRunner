namespace SqlQueryRunner.Models
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        public string ConnectionString { get; set; } = "Server=.;Database=TestDB;Integrated Security=true;TrustServerCertificate=true;";

        /// <summary>
        /// Путь к папке с SQL файлами
        /// </summary>
        public string SqlFilesPath { get; set; } = "./SqlQueries";

        /// <summary>
        /// Путь для экспорта по умолчанию
        /// </summary>
        public string DefaultExportPath { get; set; } = "./Exports";

        /// <summary>
        /// Автоматическое обновление списка файлов
        /// </summary>
        public bool AutoRefreshFiles { get; set; } = true;

        /// <summary>
        /// Таймаут выполнения запроса в секундах
        /// </summary>
        public int QueryTimeout { get; set; } = 30;

        /// <summary>
        /// Проверяет валидность настроек
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ConnectionString) &&
                   !string.IsNullOrWhiteSpace(SqlFilesPath) &&
                   !string.IsNullOrWhiteSpace(DefaultExportPath) &&
                   QueryTimeout > 0;
        }

        /// <summary>
        /// Создает копию настроек
        /// </summary>
        public AppSettings Clone()
        {
            return new AppSettings
            {
                ConnectionString = ConnectionString,
                SqlFilesPath = SqlFilesPath,
                DefaultExportPath = DefaultExportPath,
                AutoRefreshFiles = AutoRefreshFiles,
                QueryTimeout = QueryTimeout
            };
        }
    }
}