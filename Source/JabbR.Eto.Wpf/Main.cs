using System;
using Eto;
using Eto.Platform.Wpf.Forms;
using sw = System.Windows;
using Eto.Forms;
using JabbR.Eto.Client.Controls;
using Eto.Drawing;
using System.Deployment.Application;
using System.Threading.Tasks;
using Eto.Platform.Wpf.Forms.Controls;

namespace JabbR.Eto.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			var generator = Generator.Detect;
			generator.AddAssembly (typeof (MainClass).Assembly);
			generator.Add <IWebView> (typeof(Controls.CefSharpWebViewHandler));

			Style.Add<Controls.CustomForm> (null, handler => {
				AddResources (handler.Control);
			});
			Style.Add<Controls.CustomDialog> (null, handler => {
				AddResources (handler.Control);
			});
			Style.Add<TreeViewHandler> ("channelList", handler => {
				handler.Control.BorderThickness = new sw.Thickness (0);
			});
			Style.Add<TreeViewHandler> ("userList", handler => {
				handler.Control.BorderThickness = new sw.Thickness (0);
			});

			var app = new JabbRApplication();
			//app.Initialized += CheckForNewVersion;
			app.Run (args);
		}

		static void AddResources (System.Windows.Window window)
		{
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml", UriKind.RelativeOrAbsolute) });
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute) });
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml", UriKind.RelativeOrAbsolute) });
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) });
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) });
			window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/JabbReto;component/Controls.Menu.xaml", UriKind.RelativeOrAbsolute) });
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
