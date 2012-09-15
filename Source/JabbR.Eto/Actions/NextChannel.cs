using System;
using Eto.Forms;
using JabbR.Eto.Interface;

namespace JabbR.Eto.Actions
{
	public class NextChannel : ButtonAction
	{
		public const string ActionID = "NextChannel";
		
		public Channels Channels { get; private set; }
		
		public NextChannel (Channels channels)
		{
			this.Channels = channels;
			this.ID = ActionID;
			this.MenuText = "Next Channel";
			this.Accelerator = Application.Instance.CommonModifier | Key.Shift | Key.Down;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToNextChannel (false);
		}
	}
}

