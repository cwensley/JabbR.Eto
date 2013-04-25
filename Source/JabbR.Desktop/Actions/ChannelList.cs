using System;
using Eto.Forms;
using JabbR.Desktop.Model;
using JabbR.Desktop.Interface;

namespace JabbR.Desktop.Actions
{
    public class ChannelList : ButtonAction
    {
        Channels channels;
        public const string ActionID = "ChannelList";

        public ChannelList(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Channel List...";
            this.Accelerator = Application.Instance.CommonModifier | Key.L;
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
        
        protected override void OnActivated(EventArgs e)
        {
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

