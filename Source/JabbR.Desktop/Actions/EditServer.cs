using System;
using Eto.Forms;
using JabbR.Desktop.Interface.Dialogs;
using JabbR.Desktop.Interface;
using JabbR.Desktop.Model;
using System.Diagnostics;

namespace JabbR.Desktop.Actions
{
    public class EditServer : Command
    {
        public const string ActionID = "EditServer";
        Channels channels;

        public EditServer(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Edit Server...";
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedServer != null;
            }
            set
            {
                base.Enabled = value;
            }
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            var server = channels.SelectedServer;
            if (server != null)
            {
                using (var dialog = new ServerDialog(server, false, true))
                {
                    dialog.DisplayMode = DialogDisplayMode.Attached;
                    var ret = dialog.ShowDialog(Application.Instance.MainForm);
                    if (ret == DialogResult.Ok)
                    {
                        JabbRApplication.Instance.SaveConfiguration();
                        Debug.WriteLine(string.Format("Edited Server, Name: {0}", server.Name));
                    }
                }
            }
        }
    }
}

