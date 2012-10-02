using System;
using Eto;
using Eto.Platform.Wpf.Forms;
using sw = System.Windows;
using Eto.Forms;
using JabbR.Eto.Client.Controls;
using Eto.Drawing;
using System.Deployment.Application;
using System.Threading.Tasks;

namespace JabbR.Eto.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Generator.Detect.AddAssembly (typeof (MainClass).Assembly);

			Style.Add<ApplicationHandler> ("application", handler => {
				/**/
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) });
				handler.Control.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/JabbReto;component/Controls.Menu.xaml", UriKind.RelativeOrAbsolute) });
				/**/
			});

			var app = new JabbRApplication();
			//app.Initialized += CheckForNewVersion;
			app.Run (args);
		}

		static void CheckForNewVersion (object sender, EventArgs e)
		{
			if (ApplicationDeployment.IsNetworkDeployed)
			{
				var ad = ApplicationDeployment.CurrentDeployment;

				Task.Factory.StartNew (() => {
					try
					{
						var info = ad.CheckForDetailedUpdate ();

						if (info.UpdateAvailable)
						{
							var app = sender as Application;
							MessageBox.Show (app.MainForm, string.Format ("An update to version {0} is available (you have {1}). Restart the app to install!", info.AvailableVersion, ad.CurrentVersion));
						}
					}
					catch (Exception)
					{
					}
				});
			}
		}

	}
}
