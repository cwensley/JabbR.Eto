using Eto.Platform.Wpf.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sw = System.Windows;

namespace JabbR.Eto.Wpf.Controls
{
	public class CustomForm : FormHandler
	{
		public override sw.Window CreateControl ()
		{
			return new MahApps.Metro.Controls.MetroWindow ();
		}

		protected override void UpdateClientSize (global::Eto.Drawing.Size size)
		{
			// mahapps' window has oddball sizing
			var ydiff = Control.ActualHeight - Content.ActualHeight;
			Control.Width = size.Width;
			Control.Height = size.Height + ydiff - 4;
			Control.SizeToContent = sw.SizeToContent.Manual;
		}
	}
}
