using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

namespace JabbR.Eto.Interface.Dialogs
{
    public class AboutDialog : Dialog
    {
        public AboutDialog()
        {
            this.MinimumSize = new Size(250, 250);
            
            var smallFont = new Font(SystemFont.Default, 10);
            var largeFont = new Font(SystemFont.Bold, 14);

            var version = GetType().Assembly.GetName().Version;
            var versionString = string.Format("Version {0}.{1} ({2}.{3})", version.Major, version.Minor, version.Build, version.Revision);
            
            
            var layout = new DynamicLayout(this);
            
            layout.AddCentered(new ImageView { Image = Icon.FromResource ("JabbR.Eto.Resources.JabbReto.ico"), Size = new Size(128, 128) }, yscale: true);

            layout.AddCentered(new Label { Text = Application.Instance.Name, Font = largeFont });
            
            layout.AddCentered(new Label { Text = versionString, Font = smallFont }, new Padding(2));

            layout.AddCentered(new Label { Text = "Copyright © 2012 Curtis Wensley", Font = smallFont }, new Padding(2));

            if (!Generator.IsMac)
            {
                layout.AddCentered(CloseButton());
            }
        }
        
        Control CloseButton()
        {
            var control = new Button{ Text = "Close" };
            control.Click += delegate
            {
                Close();
            };
            this.DefaultButton = this.AbortButton = control;
            return control;
        }
    }
}

