using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class PrevUnreadChannel : Command
    {
        public const string ActionID = "PrevUnreadChannel";
        
        public Channels Channels { get; private set; }
        
        public PrevUnreadChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Previous Unread Channel";
            if (channels.Generator.IsMac)
                this.Shortcut = Application.Instance.CommonModifier | Keys.Up;
            else
                this.Shortcut = Keys.Alt | Keys.Up;
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Channels.GoToPreviousChannel(true);
        }
    }
}

