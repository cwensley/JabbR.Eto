using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;

namespace JabbR.Desktop.Actions
{
    public class NextChannel : Command
    {
        public const string ActionID = "NextChannel";
        
        public Channels Channels { get; private set; }
        
        public NextChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Next Channel";
            if (channels.Generator.IsMac)
                this.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.Down;
            else
                this.Shortcut = Keys.Alt | Keys.Shift | Keys.Down;
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Channels.GoToNextChannel(false);
        }
    }
}

