using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishing_Supply_Store_Inventory_System
{
    static class Program
    {
        public static string ApiBase = "http://localhost:3000";
        public static string CurrentUser = "";
        public static string CurrentRole = "";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
