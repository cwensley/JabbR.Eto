using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;

namespace JabbR.Eto.Actions
{
	public class LeaveChannel : ButtonAction
	{
		public const string ActionID = "LeaveChannel";
		
		Channels channels;
		
		public LeaveChannel (Channels channels)
		{
			this.channels = channels;
			this.ID = ActionID;
			this.MenuText = "Leave Channel";
			this.Accelerator = Application.Instance.CommonModifier | Key.Backspace;
		}
		
		public override bool Enabled {
			get {
				return base.Enabled && channels.SelectedChannel != null;
			}
			set {
				base.Enabled = value;
			}
		}
		
		protected override void OnActivated (EventArgs e)
		{
			var channel = channels.SelectedChannel;
			if (channel != null) {
				channel.Server.LeaveChannel (channel.Name);
			}
		}
	}
}

