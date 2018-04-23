using System;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System.Windows.Forms;

namespace Q296004 {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(AccessConnectionProvider.GetConnectionString(@"..\..\Q296004.mdb"),
                AutoCreateOption.DatabaseAndSchema);
            Application.Run(new Form1());
        }
    }
}