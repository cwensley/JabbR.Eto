using System;
using Eto.Forms;
using JabbR.Eto.Interface;

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
			this.Accelerator = Application.Instance.CommonModifier | Key.Down;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToNextChannel (true);
		}
	}
}

