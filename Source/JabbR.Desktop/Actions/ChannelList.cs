using System;
using Eto.Forms;
using JabbR.Desktop.Model;
using JabbR.Desktop.Interface;

namespace JabbR.Desktop.Actions
{
    public class ChannelList : Command
    {
        Channels channels;
        public const string ActionID = "ChannelList";

        public ChannelList(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Channel List...";
            this.Shortcut = Application.Instance.CommonModifier | Keys.L;
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedServer != null && channels.SelectedServer.IsConnected;
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
            if (!server.IsConnected)
                return;
            
            var dialog = new Interface.Dialogs.ChannelListDialog(server);
            if (dialog.ShowDialog(Application.Instance.MainForm) == DialogResult.Ok)
            {
                if (dialog.SelectedChannel != null)
                    channels.JoinChannel(dialog.SelectedChannel.Server, dialog.SelectedChannel.Name);
            }
        }
    }
}

