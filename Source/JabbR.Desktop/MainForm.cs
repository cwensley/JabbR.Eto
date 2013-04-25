using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Eto.Interface.Dialogs;
using JabbR.Client;
using Eto;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using JabbR.Eto.Model;
using Newtonsoft.Json;
using JabbR.Eto.Interface;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JabbR.Eto
{
    public class MainForm : Form, IXmlReadable
    {
        TopSection top;
        Configuration config;
        const string DEFAULT_TITLE = "JabbReto";
        
        public MainForm(Configuration config)
        {
            this.config = config;
            this.Title = DEFAULT_TITLE;
            this.ClientSize = new Size(1000, 600);
            this.MinimumSize = new Size(640, 400);
            this.Style = "mainForm";
            top = new TopSection(config);
            CreateActions();
            this.AddDockedControl(top);
            HandleEvent(ShownEvent);
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
            var args = new GenerateActionArgs();
            
            Application.Instance.GetSystemActions(args, true);
            
            //top.GetActions(args);
            
            args.Actions.Add(new Actions.AddServer { AutoConnect = true });
            args.Actions.Add(new Actions.EditServer(top.Channels));
            args.Actions.Add(new Actions.RemoveServer(top.Channels, config));
            args.Actions.Add(new Actions.ServerConnect(top.Channels));
            args.Actions.Add(new Actions.ServerDisconnect(top.Channels));
            args.Actions.Add(new Actions.ChannelList(top.Channels));
            args.Actions.Add(new Actions.Quit());
            args.Actions.Add(new Actions.About());
            args.Actions.Add(new Actions.ShowPreferences(config));
            
            var file = args.Menu.FindAddSubMenu("&File", 100);
            var help = args.Menu.FindAddSubMenu("&Help", 900);
            var server = args.Menu.FindAddSubMenu("&Server", 500);
            var view = args.Menu.FindAddSubMenu("&View", 500);
            

            server.Actions.Add(Actions.ServerConnect.ActionID);
            server.Actions.Add(Actions.ServerDisconnect.ActionID);
            server.Actions.AddSeparator();
            server.Actions.Add(Actions.AddServer.ActionID);
            server.Actions.Add(Actions.EditServer.ActionID);
            server.Actions.Add(Actions.RemoveServer.ActionID);
            server.Actions.AddSeparator();
            server.Actions.Add(Actions.ChannelList.ActionID);
            
            if (Generator.IsMac)
            {
                var application = args.Menu.FindAddSubMenu(Application.Instance.Name, 100);
                application.Actions.Add(Actions.About.ActionID, 100);
#if DEBUG
                // TODO: not yet implemented!
                application.Actions.Add (Actions.ShowPreferences.ActionID);
#endif
                application.Actions.Add(Actions.Quit.ActionID, 900);
            }
            else
            {
                file.Actions.Add(Actions.Quit.ActionID, 900);
                view.Actions.Add(Actions.ShowPreferences.ActionID);
                help.Actions.Add(Actions.About.ActionID, 100);
            }
            
            top.CreateActions(args);
            
            this.Menu = args.Menu.GenerateMenuBar();
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

