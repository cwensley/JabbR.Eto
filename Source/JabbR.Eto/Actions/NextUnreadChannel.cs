using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;

namespace JabbR.Eto.Actions
{
	public class NextUnreadChannel : ButtonAction
	{
		public const string ActionID = "NextUnreadChannel";
		
		public Channels Channels { get; private set; }
		
		public NextUnreadChannel (Channels channels)
		{
			this.Channels = channels;
			this.ID = ActionID;
			this.MenuText = "Next Unread Channel";
			if (channels.Generator.IsMac ())
				this.Accelerator = Application.Instance.CommonModifier | Key.Down;
			else
				this.Accelerator = Key.Alt | Key.Down;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToNextChannel (true);
		}
	}
}

