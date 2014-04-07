using Eto.Platform.Wpf.Forms;
using JabbR.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sw = System.Windows;

namespace JabbR.Windows.Controls
{
	public class CustomForm : FormHandler
	{
		public override sw.Window CreateControl ()
		{
			if (JabbRApplication.Instance.UseMetroTheme)
				return new MahApps.Metro.Controls.MetroWindow ();
			else
				return new sw.Window ();
		}
	}
}
