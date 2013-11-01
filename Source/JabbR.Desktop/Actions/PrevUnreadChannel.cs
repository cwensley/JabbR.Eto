using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class PrevUnreadChannel : ButtonAction
    {
        public const string ActionID = "PrevUnreadChannel";
        
        public Channels Channels { get; private set; }
        
        public PrevUnreadChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Previous Unread Channel";
            if (channels.Generator.IsMac)
                this.Accelerator = Application.Instance.CommonModifier | Key.Up;
            else
                this.Accelerator = Key.Alt | Key.Up;
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Channels.GoToPreviousChannel(true);
        }
    }
}

