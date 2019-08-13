using System;
using System.Windows.Forms;

namespace NanoVNA
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			MainForm mainForm = new MainForm();
			mainForm.StartPosition = FormStartPosition.CenterScreen;
			new IController(mainForm);
			Application.Run(mainForm);
		}
	}
}
