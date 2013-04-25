using System;
using Eto;
using Eto.Platform.Wpf.Forms;
using sw = System.Windows;
using Eto.Forms;
using Eto.Drawing;
using System.Deployment.Application;
using System.Threading.Tasks;
using Eto.Platform.Wpf.Forms.Controls;
using JabbR.Desktop;

namespace JabbR.Windows
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			var generator = new global::Eto.Platform.Wpf.Generator ();
			generator.Add<IDialog> (() => new Controls.CustomDialog ());
			generator.Add<IForm> (() => new Controls.CustomForm ());
            //generator.Add<IWebView>(() => new Controls.CefSharpWebViewHandler());
            generator.Add<IWebView>(() => new Controls.CefGlueWebViewHandler());
            generator.Add<IJabbRApplication>(() => new Controls.JabbRApplicationHandler());
            Generator.Initialize(generator);

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
			app.Initialized += CheckForNewVersion;
			app.Run (args);
		}

		static void AddResources (System.Windows.Window window)
		{
			if (JabbRApplication.Instance.Configuration.UseMetroTheme) {
				window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml", UriKind.RelativeOrAbsolute) });
				window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml", UriKind.RelativeOrAbsolute) });
				window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml", UriKind.RelativeOrAbsolute) });
				window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) });
				window.Resources.MergedDictionaries.Add (new sw.ResourceDictionary { Source = new Uri ("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) });
			}
		}

		static void CheckForNewVersion (object sender, EventArgs e)
		{
			if (ApplicationDeployment.IsNetworkDeployed)
			{
				var ad = ApplicationDeployment.CurrentDeployment;
				ad.CheckForUpdateCompleted += ad_CheckForUpdateCompleted;
				ad.CheckForUpdateAsync ();
			}
		}

		static void ad_CheckForUpdateCompleted (object sender, CheckForUpdateCompletedEventArgs e)
		{
			if (e.UpdateAvailable) {
				Application.Instance.AsyncInvoke (() => {
					var ad = ApplicationDeployment.CurrentDeployment;
					MessageBox.Show (Application.Instance.MainForm, string.Format ("An update to version {0} is available (you have {1}). Restart the app to install!", e.AvailableVersion, ad.CurrentVersion));
				});
			}
		}

	}
}
