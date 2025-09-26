using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SqlQueryRunner.Models;
using SqlQueryRunner.Services;

namespace SqlQueryRunner.Forms
{
    /// <summary>
    /// Главная форма приложения
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ConfigurationService _configService;
        private readonly SqlFileService _sqlFileService;
        private readonly DeclareBlockParser _declareParser;
        private readonly SqlExecutor _sqlExecutor; // Пока заглушка
        
        private AppSettings _settings;
        private List<QueryInfo> _availableQueries;

        // UI Controls
        private ListBox _queriesListBox;
        private Button _executeButton;
        private Button _refreshButton;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _connectionStatusLabel;
        private ToolStripStatusLabel _queriesCountLabel;

        public MainForm()
        {
            _configService = new ConfigurationService();
            _sqlFileService = new SqlFileService();
            _declareParser = new DeclareBlockParser();
            _sqlExecutor = new SqlExecutor(); // Заглушка из предыдущих фаз
            
            _availableQueries = new List<QueryInfo>();

            InitializeComponent();
            LoadSettings();
            InitializeUI();
            LoadQueries();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Настройки формы
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 450);
            Text = "SQL Query Runner";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(500, 350);

            ResumeLayout(false);
        }

        private void InitializeUI()
        {
            CreateMainControls();
            CreateStatusStrip();
            SetupLayout();
            SetupEventHandlers();
        }

        private void CreateMainControls()
        {
            // ListBox для списка запросов
            _queriesListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                DisplayMember = "DisplayName",
                Font = new Font("Consolas", 9F),
                IntegralHeight = false,
                SelectionMode = SelectionMode.One
            };

            // Панель с кнопками
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10)
            };

            _executeButton = new Button
            {
                Text = "Выполнить запрос",
                Size = new Size(130, 30),
                Anchor = AnchorStyles.Left,
                Enabled = false,
                Font = new Font(Font, FontStyle.Bold)
            };

            _refreshButton = new Button
            {
                Text = "Обновить список",
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Right,
                Location = new Point(buttonPanel.Width - 130, 10)
            };

            buttonPanel.Controls.Add(_executeButton);
            buttonPanel.Controls.Add(_refreshButton);

            // Главная панель
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(_queriesListBox);
            mainPanel.Controls.Add(buttonPanel);

            Controls.Add(mainPanel);
        }

        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip();

            _connectionStatusLabel = new ToolStripStatusLabel
            {
                Text = "Подключение: не проверено",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _queriesCountLabel = new ToolStripStatusLabel
            {
                Text = "Запросов: 0"
            };

            _statusStrip.Items.Add(_connectionStatusLabel);
            _statusStrip.Items.Add(_queriesCountLabel);

            Controls.Add(_statusStrip);
        }

        private void SetupLayout()
        {
            _refreshButton.Location = new Point(
                _refreshButton.Parent.Width - _refreshButton.Width - 10,
                (_refreshButton.Parent.Height - _refreshButton.Height) / 2
            );

            _executeButton.Location = new Point(
                10,
                (_executeButton.Parent.Height - _executeButton.Height) / 2
            );
        }

        private void SetupEventHandlers()
        {
            _queriesListBox.SelectedIndexChanged += QueriesListBox_SelectedIndexChanged;
            _queriesListBox.DoubleClick += QueriesListBox_DoubleClick;
            _executeButton.Click += ExecuteButton_Click;
            _refreshButton.Click += RefreshButton_Click;
            
            // Горячие клавиши
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            
            // Изменение размера
            Resize += MainForm_Resize;
        }

        private void LoadSettings()
        {
            try
            {
                _settings = _configService.LoadSettings();
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка загрузки настроек: {ex.Message}\nБудут использованы настройки по умолчанию.",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                
                _settings = _configService.GetDefaultSettings();
            }
        }

        private void LoadQueries()
        {
            try
            {
                if (!Directory.Exists(_settings.SqlFilesPath))
                {
                    Directory.CreateDirectory(_settings.SqlFilesPath);
                    ShowFirstRunMessage();
                }

                var sqlFiles = _sqlFileService.GetSqlFiles(_settings.SqlFilesPath);
                _availableQueries = sqlFiles.Select(CreateQueryInfo).ToList();

                UpdateQueriesList();
                UpdateQueriesCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка загрузки SQL файлов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                
                _availableQueries.Clear();
                UpdateQueriesList();
                UpdateQueriesCount();
            }
        }

        private QueryInfo CreateQueryInfo(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var query = new QueryInfo
            {
                Name = fileName,
                FilePath = filePath,
                DisplayName = fileName.Replace("_", " ")
            };

            // Пытаемся прочитать файл и извлечь параметры
            try
            {
                var content = File.ReadAllText(filePath);
                var parameters = _declareParser.ParseSqlFile(content);
                
                query.Parameters = parameters;
                query.ParametersCount = parameters.Count;
                query.HasAnnotations = parameters.Any(p => !string.IsNullOrEmpty(p.DisplayName));
            }
            catch (Exception ex)
            {
                query.Error = $"Ошибка чтения файла: {ex.Message}";
            }

            return query;
        }

        private void UpdateQueriesList()
        {
            _queriesListBox.BeginUpdate();
            _queriesListBox.DataSource = null;
            _queriesListBox.DataSource = _availableQueries;
            _queriesListBox.EndUpdate();

            _executeButton.Enabled = false;
        }

        private void UpdateQueriesCount()
        {
            var totalQueries = _availableQueries.Count;
            var queriesWithParams = _availableQueries.Count(q => q.ParametersCount > 0);
            var queriesWithAnnotations = _availableQueries.Count(q => q.HasAnnotations);

            _queriesCountLabel.Text = $"Запросов: {totalQueries} " +
                                     $"(с параметрами: {queriesWithParams}, " +
                                     $"с аннотациями: {queriesWithAnnotations})";
        }

        private void UpdateConnectionStatus()
        {
            // Пока заглушка - в следующих фазах будет реальная проверка
            _connectionStatusLabel.Text = "Подключение: не проверено";
            _connectionStatusLabel.ForeColor = Color.Gray;
        }

        private void ShowFirstRunMessage()
        {
            var message = $"Добро пожаловать в SQL Query Runner!\n\n" +
                         $"Папка для SQL файлов создана: {_settings.SqlFilesPath}\n\n" +
                         $"Поместите ваши .sql файлы в эту папку и нажмите 'Обновить список'.\n\n" +
                         $"Пример формата SQL файла:\n" +
                         $"-- @param StartDate \"Дата начала\" \"Описание параметра\"\n" +
                         $"DECLARE @StartDate DATE = '2024-01-01'\n" +
                         $"SELECT * FROM Sales WHERE SaleDate >= @StartDate";

            MessageBox.Show(message, "Первый запуск", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Event Handlers
        private void QueriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedQuery = _queriesListBox.SelectedItem as QueryInfo;
            _executeButton.Enabled = selectedQuery != null && string.IsNullOrEmpty(selectedQuery.Error);
        }

        private void QueriesListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_executeButton.Enabled)
            {
                ExecuteSelectedQuery();
            }
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            ExecuteSelectedQuery();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadQueries();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    LoadQueries();
                    e.Handled = true;
                    break;
                    
                case Keys.F9:
                    if (_executeButton.Enabled)
                        ExecuteSelectedQuery();
                    e.Handled = true;
                    break;
                    
                case Keys.Enter:
                    if (_queriesListBox.Focused && _executeButton.Enabled)
                    {
                        ExecuteSelectedQuery();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            SetupLayout();
        }

        private void ExecuteSelectedQuery()
        {
            var selectedQuery = _queriesListBox.SelectedItem as QueryInfo;
            if (selectedQuery == null || !string.IsNullOrEmpty(selectedQuery.Error))
                return;

            try
            {
                // Если есть параметры, показываем форму ввода
                if (selectedQuery.Parameters.Any())
                {
                    using var paramForm = new ParameterInputForm(selectedQuery.Parameters, selectedQuery.Name);
                    
                    if (paramForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // В следующей фазе здесь будет выполнение запроса
                        ShowParametersPreview(selectedQuery, paramForm.ParameterValues);
                    }
                }
                else
                {
                    // Запрос без параметров - в следующей фазе будет выполнение
                    MessageBox.Show(
                        $"Запрос '{selectedQuery.Name}' будет выполнен без параметров.\n\n" +
                        "Выполнение запросов будет реализовано в следующей фазе.",
                        "Информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при подготовке запроса: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ShowParametersPreview(QueryInfo query, Dictionary<string, object?> parameters)
        {
            var preview = $"Запрос: {query.Name}\n\n";
            preview += "Параметры:\n";
            
            foreach (var param in parameters)
            {
                var paramInfo = query.Parameters.First(p => p.Name == param.Key);
                preview += $"• {paramInfo.GetDisplayName()}: {param.Value ?? "NULL"}\n";
            }

            preview += "\nВыполнение запросов будет реализовано в следующей фазе (Фаза 4).";

            MessageBox.Show(preview, "Предварительный просмотр", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Освобождаем ресурсы
            }
            base.Dispose(disposing);
        }
    }

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

        public override string ToString() => DisplayName;
    }
}