using System;
using System.Windows.Forms;

namespace DetectorBordes
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDetectorBordes());
        }
    }
}