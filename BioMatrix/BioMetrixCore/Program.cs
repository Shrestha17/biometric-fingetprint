using System;
using System.Windows.Forms;

namespace BioMetrixCore
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainFile.MainClass());
            //Application.Run(new Main.main());
            Application.Run(new Master());
            //var MainClassObj = new MainFile.MainClass();
            //Application.Run(MainClassObj);
        }
    }
}
