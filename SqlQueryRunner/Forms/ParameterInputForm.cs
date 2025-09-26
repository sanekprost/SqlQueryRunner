using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Forms
{
    /// <summary>
    /// Упрощенная форма ввода параметров для тестирования
    /// </summary>
    public partial class ParameterInputForm : Form
    {
        private readonly List<ParameterInfo> _parameters;
        
        public Dictionary<string, object?> ParameterValues { get; private set; }

        public ParameterInputForm(List<ParameterInfo> parameters, string queryName = "")
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ParameterValues = new Dictionary<string, object?>();

            InitializeComponent();
            SetupForm(queryName);
            CreateParameterControls();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Настройки формы
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            ShowInTaskbar = false;

            ResumeLayout(false);
        }

        private void SetupForm(string queryName)
        {
            Text = string.IsNullOrEmpty(queryName) ? "Параметры запроса" : $"Параметры: {queryName}";
        }

        private void CreateParameterControls()
        {
            if (!_parameters.Any())
            {
                ShowNoParametersMessage();
                return;
            }

            // Создаем простой список параметров
            var listBox = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(400, 200),
                Font = new Font("Consolas", 9F)
            };

            var items = new List<string>();
            foreach (var param in _parameters)
            {
                var displayName = string.IsNullOrEmpty(param.DisplayName) ? param.Name : param.DisplayName;
                var defaultValue = param.HasDefault ? $" = {param.DefaultValue}" : "";
                items.Add($"@{param.Name} ({param.SqlType}){defaultValue} - {displayName}");
                
                // Добавляем значения по умолчанию
                ParameterValues[param.Name] = param.DefaultValue;
            }

            listBox.DataSource = items;
            Controls.Add(listBox);

            // Создаем кнопки
            CreateButtons(240);
            AdjustFormSize(300);
        }

        private void CreateButtons(int yPosition)
        {
            var buttonWidth = 100;
            var buttonHeight = 30;
            var spacing = 10;
            var totalButtonWidth = buttonWidth * 2 + spacing;
            var startX = (ClientSize.Width - totalButtonWidth) / 2;

            // Кнопка "Выполнить"
            var executeButton = new Button
            {
                Text = "Выполнить",
                Location = new Point(startX, yPosition),
                Size = new Size(buttonWidth, buttonHeight),
                DialogResult = DialogResult.OK,
                UseVisualStyleBackColor = true,
                Font = new Font(Font, FontStyle.Bold)
            };
            Controls.Add(executeButton);

            // Кнопка "Отмена"
            var cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(startX + buttonWidth + spacing, yPosition),
                Size = new Size(buttonWidth, buttonHeight),
                DialogResult = DialogResult.Cancel,
                UseVisualStyleBackColor = true
            };
            Controls.Add(cancelButton);

            // Устанавливаем кнопку по умолчанию
            AcceptButton = executeButton;
            CancelButton = cancelButton;
        }

        private void ShowNoParametersMessage()
        {
            var label = new Label
            {
                Text = "У данного запроса нет параметров для ввода.",
                Location = new Point(20, 50),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Italic)
            };
            Controls.Add(label);

            CreateButtons(120);
            AdjustFormSize(180);
        }

        private void AdjustFormSize(int requiredHeight)
        {
            var newHeight = Math.Max(requiredHeight, 180);
            ClientSize = new Size(ClientSize.Width, newHeight);
        }
    }
}