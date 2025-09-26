using System;
using System.Windows.Forms;
using SqlQueryRunner.Forms;

namespace SqlQueryRunner
{
    /// <summary>
    /// Главный класс приложения
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Настройка приложения для высокого DPI
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Запуск главной формы
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Критическая ошибка приложения:\n\n{ex.Message}\n\nПриложение будет закрыто.",
                    "Критическая ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}