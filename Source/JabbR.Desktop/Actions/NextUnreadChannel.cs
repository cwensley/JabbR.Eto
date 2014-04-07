using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class NextUnreadChannel : Command
    {
        public const string ActionID = "NextUnreadChannel";
        
        public Channels Channels { get; private set; }
        
        public NextUnreadChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Next Unread Channel";
            if (channels.Generator.IsMac)
                this.Shortcut = Application.Instance.CommonModifier | Keys.Down;
            else
                this.Shortcut = Keys.Alt | Keys.Down;
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Channels.GoToNextChannel(true);
        }
    }
}

