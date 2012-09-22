using System;
using Eto;
using Eto.Platform.Wpf.Forms;
using sw = System.Windows;
using Eto.Forms;
using JabbR.Eto.Client.Controls;

namespace JabbR.Eto.Wpf
{
	public class CustomForm : FormHandler
	{
		public override sw.Window CreateControl ()
		{
			return new MahApps.Metro.Controls.MetroWindow ();
		}
	}
	public class CustomDialog : DialogHandler
	{
		public override sw.Window CreateControl ()
		{
			return new MahApps.Metro.Controls.MetroWindow ();
		}
	}
	
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Generator.Detect.AddAssembly (typeof (MainClass).Assembly);

			Style.Add<ApplicationHandler> ("application", handler => {
				/**/
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/JabbR.Eto.Wpf;component/Controls.Menu.xaml", UriKind.RelativeOrAbsolute) });
				/**/
			});

			var app = new JabbRApplication();
			app.Run (args);
		}
	}
}
