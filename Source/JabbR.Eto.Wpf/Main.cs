using System;
using Eto;
using Eto.Platform.Wpf.Forms;
using sw = System.Windows;
using Eto.Forms;
using JabbR.Eto.Client.Controls;
using Eto.Drawing;

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

		class JabbRApplication2 : Application
		{
			TextBox text;
			TreeView tree;

			public override void OnInitialized (EventArgs e)
			{
				base.OnInitialized (e);

				this.MainForm = new Form ();

				this.MainForm.ClientSize = new Size (300, 300);
				var layout = new DynamicLayout (this.MainForm);

				layout.Add (TreeView (), yscale: true);
				layout.Add (TextBox ());

				this.MainForm.Show ();

			}

			void CreateItems (TreeItemCollection items, int count, int level)
			{
				for (int i = 0; i < count; i++)
				{
					var item = new TreeItem
					{
						Text = "Item " + level + " " + i,
						Expanded = true
					};
					if (level < 2)
						CreateItems (item.Children, 3, level + 1);
					items.Add (item);
				}
			}

			Control TreeView ()
			{
				tree = new TreeView ();
				tree.Size = new Size (100, 200);
				tree.SelectionChanged += (sender, e) => {
					tree.RefreshData ();
					text.Text = tree.SelectedItem.Text;
					text.Focus ();
				};

				var items = new TreeItemCollection ();

				CreateItems (items, 10, 0);

				tree.DataStore = items;
				return tree;
			}

			Control TextBox ()
			{
				text = new TextBox ();
				return text;
			}
		}
	}
}
