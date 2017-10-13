using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace CAP_JADE_Interface
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //process Name
            string strProcessName = Process.GetCurrentProcess().ProcessName;
            //verify if its running
            Process[] processes= Process.GetProcessesByName(strProcessName);
            if (processes.Length > 1)
            {
                MessageBox.Show("The Application is already running.");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                //Application.Run(new PendingOrders());
            }
        }
    }
}
