using System;
using Eto.Platform.Mac.Forms;


namespace JabbR.Eto.Mac.Controls
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		public JabbRApplicationHandler ()
		{
		}

		public string BadgeLabel {
			get { return Control.DockTile.BadgeLabel; }
			set { Control.DockTile.BadgeLabel = value ?? string.Empty; }
		}
	}
}

