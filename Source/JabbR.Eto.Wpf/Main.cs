using System;
using Eto;

namespace JabbR.Eto.Client
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Generator.Detect.AddAssembly (typeof (MainClass).Assembly);

			var app = new JabbRApplication();
			app.Run (args);
		}
	}
}
