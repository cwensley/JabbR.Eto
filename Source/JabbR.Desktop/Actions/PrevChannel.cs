using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class PrevChannel : Command
    {
        public const string ActionID = "PrevChannel";
        
        public Channels Channels { get; private set; }
        
        public PrevChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Previous Channel";
            if (channels.Generator.IsMac)
                this.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.Up;
            else
                this.Shortcut = Keys.Alt | Keys.Shift | Keys.Up;
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Channels.GoToPreviousChannel(false);
        }
    }
}

