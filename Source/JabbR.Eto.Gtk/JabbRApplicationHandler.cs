using System;
using Eto.Platform.GtkSharp;


namespace JabbR.Eto.Gtk
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		public JabbRApplicationHandler ()
		{
		}
		
		public string BadgeLabel { get; set; }
	}
}

