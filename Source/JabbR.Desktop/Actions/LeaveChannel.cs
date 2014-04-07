using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class LeaveChannel : Command
    {
        public const string ActionID = "LeaveChannel";
        Channels channels;
        
        public LeaveChannel(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Leave Channel";
            this.Shortcut = Application.Instance.CommonModifier | Keys.Backspace;
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedChannel != null;
            }
            set
            {
                base.Enabled = value;
            }
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            var channel = channels.SelectedChannel;
            if (channel != null)
            {
                channel.Server.LeaveChannel(channel);
            }
        }
    }
}

