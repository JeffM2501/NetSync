using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFrame
{
    static class Program
    {
		public static Form1 TheForm = null;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main()
        {

			Application.Idle += Application_Idle;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			TheForm = new Form1();

			Application.Run(TheForm);
        }

		private static void Application_Idle(object sender, EventArgs e)
		{
			TheForm.IdleUpdate();
		}
	}
}
