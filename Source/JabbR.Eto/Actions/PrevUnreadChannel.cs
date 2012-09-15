using System;
using Eto.Forms;
using JabbR.Eto.Interface;

namespace JabbR.Eto.Actions
{
	public class PrevUnreadChannel : ButtonAction
	{
		public const string ActionID = "PrevUnreadChannel";
		
		public Channels Channels { get; private set; }
		
		public PrevUnreadChannel (Channels channels)
		{
			this.Channels = channels;
			this.ID = ActionID;
			this.MenuText = "Previous Unread Channel";
			this.Accelerator = Application.Instance.CommonModifier | Key.Up;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToPreviousChannel (true);
		}
	}
}

