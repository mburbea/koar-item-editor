using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KingdomsofAmalurReckoningSaveEditer
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
