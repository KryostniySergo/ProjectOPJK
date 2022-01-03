using System;
using System.Threading;
using System.Windows.Forms;

namespace ProjectOPJK
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Exception);
            Application.Run(new Form1());
        }

        static void Exception(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(
                $"ОТПРАВТЕ ЭТО СООБЩЕНИЕ В СПЕЦИАЛЬНУЮ ТЕМУ В ПАБЛИКЕ\n\n" +
                $"{e.Exception.ToString()}",
                "Возникла глобальная ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
