using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Platform.Wpf.Forms;

namespace JabbR.Eto.Client.Controls
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		public string BadgeLabel
		{
			get;
			set;
		}
	}
}
