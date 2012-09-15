using System;
using Eto.Forms;
using JabbR.Eto.Interface;

namespace JabbR.Eto.Actions
{
	public class PrevChannel : ButtonAction
	{
		public const string ActionID = "PrevChannel";
		
		public Channels Channels { get; private set; }
		
		public PrevChannel (Channels channels)
		{
			this.Channels = channels;
			this.ID = ActionID;
			this.MenuText = "Previous Channel";
			this.Accelerator = Application.Instance.CommonModifier | Key.Shift | Key.Up;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToPreviousChannel (false);
		}
	}
}

