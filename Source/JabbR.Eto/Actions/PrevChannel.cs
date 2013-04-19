using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;

namespace JabbR.Eto.Actions
{
    public class PrevChannel : ButtonAction
    {
        public const string ActionID = "PrevChannel";
        
        public Channels Channels { get; private set; }
        
        public PrevChannel(Channels channels)
        {
            this.Channels = channels;
            this.ID = ActionID;
            this.MenuText = "Previous Channel";
            if (channels.Generator.IsMac)
                this.Accelerator = Application.Instance.CommonModifier | Key.Shift | Key.Up;
            else
                this.Accelerator = Key.Alt | Key.Shift | Key.Up;
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Channels.GoToPreviousChannel(false);
        }
    }
}

