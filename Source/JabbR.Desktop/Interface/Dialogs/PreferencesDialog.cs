using Eto;
using Eto.Drawing;
using Eto.Forms;
using JabbR.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JabbR.Desktop.Interface.Dialogs
{
    public class PreferencesDialog : Dialog
    {
        Configuration config;

        public PreferencesDialog(Configuration config)
        {
            this.config = config;
            this.MinimumSize = new Size(200, 300);
            this.Title = "JabbR Preferences";
            this.Resizable = true;

            var layout = new DynamicLayout();

            layout.Add(Tabs(), yscale: true);
            layout.AddSeparateRow(null, this.CancelButton(), this.OkButton(clicked: SaveData));
            Content = layout;
        }

        Control Tabs()
        {
            var tabs = new TabControl();

            var layout = new DynamicLayout();
            
            var desc = string.Format("Show {0} Badge", (Generator.IsMac) ? "Dock" : "TaskBar");
            
            layout.AddRow(new Label { Text = desc }, BadgeSelector());
            if (Generator.ID == Generators.Wpf)
                layout.AddRow(new Panel(), UseMetroTheme());
            layout.AddRow();

            tabs.TabPages.Add(new TabPage { Text = "General", Content = layout });
            return tabs;
        }

        Control UseMetroTheme()
        {
            var control = new CheckBox { Text = "Use Metro Theme" };
            control.Bind(r => r.Checked, config, r => r.UseMetroTheme, DualBindingMode.OneWay);
            return control;
        }

        bool SaveData()
        {
            UpdateBindings();
            return true;
        }

        Control BadgeSelector()
        {
            var control = new EnumComboBox<BadgeDisplayMode>();

            /*
            control.Items.Add ("All Messages", BadgeDisplayMode.All.ToString());
            control.Items.Add ("Higlighted Messages only", BadgeDisplayMode.Highlighted.ToString());
            control.Items.Add ("None", BadgeDisplayMode.None.ToString ());
             */
            control.Bind(r => r.SelectedValue, config, r => r.BadgeDisplay, DualBindingMode.OneWay);
            return control;
        }
    }
}
