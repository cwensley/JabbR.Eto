using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Desktop.Interface.Dialogs;
using JabbR.Client;
using Eto;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using JabbR.Desktop.Model;
using Newtonsoft.Json;
using JabbR.Desktop.Interface;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JabbR.Desktop
{
    public class MainForm : Form, IXmlReadable
    {
        TopSection top;
        Configuration config;
        const string DEFAULT_TITLE = "JabbR";

        public MainForm(Configuration config)
        {
            this.config = config;
            this.Title = DEFAULT_TITLE;
            this.ClientSize = new Size(1000, 600);
            this.MinimumSize = new Size(640, 400);
            this.Style = "mainForm";
            top = new TopSection(config);
            CreateActions();
            Content = top;
        }

        public void SetUnreadCount(string titleLabel, int count)
        {
            var sb = new StringBuilder();
            if (count > 0)
            {
                sb.AppendFormat("{0} ({1})", DEFAULT_TITLE, count);
                Application.Instance.BadgeLabel = count.ToString();
            }
            else
            {
                sb.Append(DEFAULT_TITLE);
                Application.Instance.BadgeLabel = null;
            }
            if (!string.IsNullOrEmpty(titleLabel))
                sb.AppendFormat(" - {0}", titleLabel);
            this.Title = sb.ToString();
        }

        void CreateActions()
        {
            var menu = new MenuBar();
            Application.Instance.CreateStandardMenu(menu.Items);

            var file = menu.Items.GetSubmenu("&File", 100);
            var help = menu.Items.GetSubmenu("&Help", 900);
            var server = menu.Items.GetSubmenu("&Server", 500);
            var view = menu.Items.GetSubmenu("&View", 500);
            

            server.Items.Add(new Actions.ServerConnect(top.Channels), 500);
            server.Items.Add(new Actions.ServerDisconnect(top.Channels), 500);
            server.Items.AddSeparator(500);
            server.Items.Add(new Actions.AddServer { AutoConnect = true }, 500);
            server.Items.Add(new Actions.EditServer(top.Channels), 500);
            server.Items.Add(new Actions.RemoveServer(top.Channels, config), 500);
            server.Items.AddSeparator(500);
            server.Items.Add(new Actions.ChannelList(top.Channels), 500);
            
            if (Generator.IsMac)
            {
                var application = menu.Items.GetSubmenu(Application.Instance.Name, 100);
                application.Items.Add(new Actions.About(), 100);
#if DEBUG
                // TODO: not yet implemented!
				application.Items.Add(new Actions.ShowPreferences(config), 500);
#endif
                application.Items.Add(new Actions.Quit(), 900);
            }
            else
            {
                file.Items.Add(new Actions.Quit(), 900);
                view.Items.Add(new Actions.ShowPreferences(config), 500);
                help.Items.Add(new Actions.About(), 100);
            }
            
            top.CreateActions(menu.Items);

			menu.Items.Trim();
            Menu = menu;
        }

        public new void Initialize()
        {
            top.Initialize();
        }

        public override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
        }

        #region IXmlReadable implementation

        public void ReadXml(XmlElement element)
        {
            var bounds = element.ReadChildRectangleXml("bounds");
            if (bounds != null)
                this.Bounds = bounds.Value;
            else
            {
                var clientSize = element.ReadChildSizeXml("clientSize");
                if (clientSize != null)
                    this.ClientSize = clientSize.Value;
            }
            bool maximized = element.GetBoolAttribute("maximized") ?? false;
            if (maximized)
            {
                this.Maximize();
            }

            element.ReadChildXml("top", top);
        }

        public void WriteXml(XmlElement element)
        {
            element.WriteChildRectangleXml("bounds", this.RestoreBounds ?? this.Bounds);
            if (this.WindowState == WindowState.Maximized)
            {
                element.SetAttribute("maximized", true);
            }
            
            element.WriteChildXml("top", top);
        }

        #endregion
    }
}

