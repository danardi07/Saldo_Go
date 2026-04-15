using System.Windows.Forms;
using System;
using System.Windows.Forms;

namespace SaldoGo
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StatusKoneksi());
        }
    }
}
