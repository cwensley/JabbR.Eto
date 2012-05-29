using System;

namespace JabbR.Eto.Client
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			var app = new JabbRApplication();
			app.Run (args);
		}
	}
}
