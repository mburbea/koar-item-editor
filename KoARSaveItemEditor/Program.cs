using System;
using System.Windows.Forms;

namespace KoARSaveItemEditor
{
    static class Program
    {
        /// <summary>
        /// Main/Start
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
