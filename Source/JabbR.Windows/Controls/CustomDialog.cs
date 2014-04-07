using Eto.Platform.Wpf.Forms;
using JabbR.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using sw = System.Windows;

namespace JabbR.Windows.Controls
{
	public class CustomDialog : DialogHandler
	{

		public override sw.Window CreateControl ()
		{
			if (JabbRApplication.Instance.UseMetroTheme)
				return new MahApps.Metro.Controls.MetroWindow();
			else
				return new sw.Window();
		}

		protected override void SetResizeMode()
		{
			base.SetResizeMode();

			var win = Control as MahApps.Metro.Controls.MetroWindow;
			if (win != null)
			{
				win.ShowMinButton = Minimizable;
				win.ShowMaxRestoreButton = Resizable && Maximizable;
			}
		}
	}
}
