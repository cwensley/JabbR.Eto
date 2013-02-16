using System;
using Eto;

namespace JabbR.Eto.Gtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var generator = Generator.Detect;
			generator.Add <IJabbRApplication> (() => new JabbRApplicationHandler ());
			
			var app = new JabbRApplication();
			app.Run (args);
		}
	}
}
