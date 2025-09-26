using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SqlQueryRunner.Models;

namespace SqlQueryRunner.Forms
{
    /// <summary>
    /// Форма для динамического ввода параметров SQL запроса
    /// </summary>
    public partial class ParameterInputForm : Form
    {
        private readonly List<ParameterInfo> _parameters;
        private readonly Dictionary<string, Control> _parameterControls;
        private readonly Dictionary<string, Label> _parameterLabels;
        private readonly ToolTip _toolTip;

        public Dictionary<string, object?> ParameterValues { get; private set; }

        public ParameterInputForm(List<ParameterInfo> parameters, string queryName = "")
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _parameterControls = new Dictionary<string, Control>();
            _parameterLabels = new Dictionary<string, Label>();
            _toolTip = new ToolTip();
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
            
            // Настройка ToolTip
            _toolTip.AutoPopDelay = 10000;
            _toolTip.InitialDelay = 500;
            _toolTip.ReshowDelay = 100;
            _toolTip.ShowAlways = true;
            _toolTip.ToolTipIcon = ToolTipIcon.Info;
            _toolTip.ToolTipTitle = "Информация о параметре";
        }

        private void CreateParameterControls()
        {
            if (!_parameters.Any())
            {
                ShowNoParametersMessage();
                return;
            }

            const int startY = 20;
            const int labelHeight = 20;
            const int controlHeight = 23;
            const int spacing = 10;
            const int leftMargin = 20;
            const int labelWidth = 150;
            const int controlWidth = 250;

            int currentY = startY;

            // Создаем контролы для каждого параметра
            foreach (var parameter in _parameters)
            {
                CreateParameterControl(parameter, leftMargin, currentY, labelWidth, controlWidth);
                currentY += labelHeight + controlHeight + spacing;
            }

            // Создаем кнопки
            CreateButtons(currentY + 20);

            // Подгоняем размер формы
            AdjustFormSize(currentY + 80);
        }

        private void CreateParameterControl(ParameterInfo parameter, int x, int y, int labelWidth, int controlWidth)
        {
            // Создаем Label
            var label = new Label
            {
                Text = $"{parameter.GetDisplayName()}:",
                Location = new Point(x, y),
                Size = new Size(labelWidth, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Font.FontFamily, Font.Size, parameter.IsRequired ? FontStyle.Bold : FontStyle.Regular)
            };

            // Добавляем звездочку для обязательных параметров
            if (parameter.IsRequired)
            {
                label.Text += " *";
                label.ForeColor = Color.DarkBlue;
            }

            Controls.Add(label);
            _parameterLabels[parameter.Name] = label;

            // Создаем контрол ввода в зависимости от типа
            Control inputControl = parameter.Type switch
            {
                ParameterType.DateTime => CreateDateTimeControl(parameter, x, y + 22, controlWidth),
                ParameterType.Integer => CreateIntegerControl(parameter, x, y + 22, controlWidth),
                ParameterType.Decimal => CreateDecimalControl(parameter, x, y + 22, controlWidth),
                ParameterType.Boolean => CreateBooleanControl(parameter, x, y + 22, controlWidth),
                ParameterType.String => CreateStringControl(parameter, x, y + 22, controlWidth),
                _ => CreateStringControl(parameter, x, y + 22, controlWidth)
            };

            Controls.Add(inputControl);
            _parameterControls[parameter.Name] = inputControl;

            // Устанавливаем подсказку
            _toolTip.SetToolTip(label, parameter.GetTooltip());
            _toolTip.SetToolTip(inputControl, parameter.GetTooltip());
        }

        private DateTimePicker CreateDateTimeControl(ParameterInfo parameter, int x, int y, int width)
        {
            var control = new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm:ss",
                ShowCheckBox = !parameter.IsRequired
            };

            // Устанавливаем значение по умолчанию
            if (parameter.HasDefault)
            {
                if (parameter.DefaultValue is DateTime dateValue)
                {
                    control.Value = dateValue;
                    control.Checked = true;
                }
                else if (parameter.DefaultValue == null)
                {
                    control.Checked = false;
                }
            }
            else
            {
                control.Value = DateTime.Now;
                control.Checked = parameter.IsRequired;
            }

            return control;
        }

        private NumericUpDown CreateIntegerControl(ParameterInfo parameter, int x, int y, int width)
        {
            var control = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };

            // Устанавливаем значение по умолчанию
            if (parameter.HasDefault && parameter.DefaultValue != null)
            {
                if (parameter.DefaultValue is int intValue)
                {
                    control.Value = intValue;
                }
                else if (int.TryParse(parameter.DefaultValue.ToString(), out var parsedValue))
                {
                    control.Value = parsedValue;
                }
            }

            return control;
        }

        private NumericUpDown CreateDecimalControl(ParameterInfo parameter, int x, int y, int width)
        {
            var control = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                Minimum = decimal.MinValue,
                Maximum = decimal.MaxValue,
                DecimalPlaces = parameter.Scale ?? 2,
                ThousandsSeparator = true
            };

            // Устанавливаем значение по умолчанию
            if (parameter.HasDefault && parameter.DefaultValue != null)
            {
                if (parameter.DefaultValue is decimal decValue)
                {
                    control.Value = decValue;
                }
                else if (decimal.TryParse(parameter.DefaultValue.ToString(), out var parsedValue))
                {
                    control.Value = parsedValue;
                }
            }

            return control;
        }

        private CheckBox CreateBooleanControl(ParameterInfo parameter, int x, int y, int width)
        {
            var control = new CheckBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                Text = "Да",
                UseVisualStyleBackColor = true,
                ThreeState = !parameter.IsRequired
            };

            // Устанавливаем значение по умолчанию
            if (parameter.HasDefault)
            {
                if (parameter.DefaultValue is bool boolValue)
                {
                    control.Checked = boolValue;
                    control.CheckState = boolValue ? CheckState.Checked : CheckState.Unchecked;
                }
                else if (parameter.DefaultValue == null)
                {
                    control.CheckState = CheckState.Indeterminate;
                }
            }
            else
            {
                control.CheckState = parameter.IsRequired ? CheckState.Unchecked : CheckState.Indeterminate;
            }

            return control;
        }

        private TextBox CreateStringControl(ParameterInfo parameter, int x, int y, int width)
        {
            var control = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23),
                MaxLength = parameter.MaxLength ?? 32767
            };

            // Устанавливаем значение по умолчанию
            if (parameter.HasDefault && parameter.DefaultValue != null)
            {
                control.Text = parameter.DefaultValue.ToString();
            }

            return control;
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
            executeButton.Click += ExecuteButton_Click;
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

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            if (ValidateAndCollectValues())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateAndCollectValues()
        {
            ParameterValues.Clear();
            var validationErrors = new List<string>();

            foreach (var parameter in _parameters)
            {
                if (!_parameterControls.TryGetValue(parameter.Name, out var control))
                    continue;

                object? value = ExtractValueFromControl(control, parameter);
                var validationResult = parameter.ValidateValue(value);

                if (!validationResult.IsValid)
                {
                    validationErrors.Add(validationResult.ErrorMessage);
                    HighlightErrorControl(control);
                }
                else
                {
                    ClearErrorHighlight(control);
                    ParameterValues[parameter.Name] = value;
                }
            }

            if (validationErrors.Any())
            {
                MessageBox.Show(
                    "Обнаружены ошибки в заполнении параметров:\n\n" + string.Join("\n", validationErrors),
                    "Ошибки валидации",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            return true;
        }

        private object? ExtractValueFromControl(Control control, ParameterInfo parameter)
        {
            return control switch
            {
                DateTimePicker dtp => dtp.Checked ? (object)dtp.Value : null,
                NumericUpDown nud => (object)nud.Value,
                CheckBox cb => cb.CheckState switch
                {
                    CheckState.Checked => true,
                    CheckState.Unchecked => false,
                    CheckState.Indeterminate => null,
                    _ => null
                },
                TextBox tb => string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text.Trim(),
                _ => null
            };
        }

        private void HighlightErrorControl(Control control)
        {
            control.BackColor = Color.MistyRose;
            
            // Для Label тоже меняем цвет
            var parameter = _parameters.FirstOrDefault(p => _parameterControls[p.Name] == control);
            if (parameter != null && _parameterLabels.TryGetValue(parameter.Name, out var label))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void ClearErrorHighlight(Control control)
        {
            control.BackColor = SystemColors.Window;
            
            // Восстанавливаем цвет Label
            var parameter = _parameters.FirstOrDefault(p => _parameterControls[p.Name] == control);
            if (parameter != null && _parameterLabels.TryGetValue(parameter.Name, out var label))
            {
                label.ForeColor = parameter.IsRequired ? Color.DarkBlue : SystemColors.ControlText;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Сбрасывает все контролы к значениям по умолчанию
        /// </summary>
        public void ResetToDefaults()
        {
            foreach (var parameter in _parameters)
            {
                if (!_parameterControls.TryGetValue(parameter.Name, out var control))
                    continue;

                switch (control)
                {
                    case DateTimePicker dtp:
                        if (parameter.HasDefault && parameter.DefaultValue is DateTime dateValue)
                        {
                            dtp.Value = dateValue;
                            dtp.Checked = true;
                        }
                        else
                        {
                            dtp.Value = DateTime.Now;
                            dtp.Checked = parameter.IsRequired;
                        }
                        break;

                    case NumericUpDown nud:
                        if (parameter.HasDefault && parameter.DefaultValue != null)
                        {
                            if (decimal.TryParse(parameter.DefaultValue.ToString(), out var numValue))
                            {
                                nud.Value = numValue;
                            }
                        }
                        else
                        {
                            nud.Value = 0;
                        }
                        break;

                    case CheckBox cb:
                        if (parameter.HasDefault)
                        {
                            if (parameter.DefaultValue is bool boolValue)
                            {
                                cb.CheckState = boolValue ? CheckState.Checked : CheckState.Unchecked;
                            }
                            else
                            {
                                cb.CheckState = CheckState.Indeterminate;
                            }
                        }
                        else
                        {
                            cb.CheckState = parameter.IsRequired ? CheckState.Unchecked : CheckState.Indeterminate;
                        }
                        break;

                    case TextBox tb:
                        tb.Text = parameter.HasDefault && parameter.DefaultValue != null
                            ? parameter.DefaultValue.ToString()
                            : string.Empty;
                        break;
                }

                ClearErrorHighlight(control);
            }
        }
    }
}