using Eto.Platform.Wpf.Forms;
using JabbR.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sw = System.Windows;

namespace JabbR.Windows.Controls
{
	public class CustomDialog : DialogHandler
	{
		public override sw.Window CreateControl ()
		{
			if (JabbRApplication.Instance.Configuration.UseMetroTheme)
				return new MahApps.Metro.Controls.MetroWindow ();
			else
				return new sw.Window ();
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
